using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Xml;
using OpenFileSystem.IO;
using OpenRasta.Client;
using OpenWrap.PackageManagement.Packages;
using OpenWrap.PackageModel;
using OpenWrap.Repositories.Caching;
using OpenWrap.Repositories.Http;
using OpenWrap.Repositories.NuGet;

namespace OpenWrap.Repositories.NuFeed
{
    public enum NuFeedDownloadMode
    {
        PartialParallel,
        Partial,
        Sequential
    }
    public class NuFeedRepository : IPackageRepository, ISupportAuthentication, ISupportCaching
    {
        readonly PackageCacheManager _cacheManager;
        readonly IHttpClient _client;
        readonly IFileSystem _fileSystem;
        readonly Uri _packagesUri;
        readonly Uri _target;
        LazyValue<ILookup<string, IPackageInfo>> _packages;

        public NuFeedRepository(IFileSystem fileSystem, IHttpClient client, Uri target, Uri packagesUri) : this(fileSystem, null, client, target, packagesUri)
        {
        }

        public NuFeedRepository(IFileSystem fileSystem, PackageCacheManager cacheManager, IHttpClient client, Uri target, Uri packagesUri)
        {
            _fileSystem = fileSystem;
            _client = client;
            _target = target;
            _packagesUri = packagesUri;
            _cacheManager = cacheManager;
            CachingEnabled = _cacheManager != null;
            Mode = CachingEnabled
                ? NuFeedDownloadMode.PartialParallel 
                : NuFeedDownloadMode.Sequential;

            RefreshPackages();
        }

        protected NuFeedDownloadMode Mode { get; set; }

        protected bool ParallelDownloadEnabled { get; set; }

        public bool CachingEnabled { get; private set; }

        public NetworkCredential CurrentCredentials { get; private set; }

        public string Name
        {
            get { return "NuGet OData feed"; }
        }

        public ILookup<string, IPackageInfo> PackagesByName
        {
            get { return _packages.Value; }
        }

        public string Token
        {
            get { return string.Format("[nuget][{0}]{1}", _target, _packagesUri); }
        }

        public string Type
        {
            get { return "nufeed"; }
        }

        public TFeature Feature<TFeature>() where TFeature : class, IRepositoryFeature
        {
            if (typeof(TFeature) == typeof(ISupportCaching) && !CachingEnabled) return null;
            return this as TFeature;
        }

        public void RefreshPackages()
        {
            DateTimeOffset? lastUpdated = null;
            _packages = Lazy.Is(() => CachingEnabled
                                          ? LoadPackagesThroughCache()
                                          : LoadPackagesFromChainedFeedPages(_packagesUri, out lastUpdated)
                                                .ToLookup(_ => _.Name, x => (IPackageInfo)new PackageEntryWrapper(this, x, LoadPackage(x))));
        }

        public IDisposable WithCredentials(NetworkCredential credentials)
        {
            var oldCredentials = CurrentCredentials;
            CurrentCredentials = credentials;
            return new ActionOnDispose(() => CurrentCredentials = oldCredentials);
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public CacheState GetState()
        {
            return _cacheManager.GetState(Token);
        }

        public void Update()
        {
            var existingToken = _cacheManager.GetState(Token);

            if (existingToken.UpdateToken == null)
            {
                AppendPackagesToCache(_packagesUri);
            }
            else
            {
                var token = existingToken.UpdateToken;
                var updateUri = new UriBuilder(_packagesUri);
                updateUri.Query = string.Format("LastUpdated gt datetime'{0}'", token);
                AppendPackagesToCache(updateUri.Uri);
            }
            RefreshPackages();
        }
        
        void AppendPackagesToCache(Uri packagesUri)
        {
            DateTimeOffset? lastUpdate = null;
            var finalList = Mode == NuFeedDownloadMode.PartialParallel
                                ? LoadPackagesFromParallelFeeds(packagesUri, out lastUpdate)
                                : LoadPackagesFromChainedFeedPages(packagesUri, out lastUpdate);

            _cacheManager.AppendPackages(Token, new NuFeedToken(lastUpdate), finalList);
        }

        IEnumerable<PackageEntry> LoadPackagesFromParallelFeeds(Uri packagesUri, out DateTimeOffset? lastUpdate)
        {
            lastUpdate = null;
            var queue =
                new Queue<Uri>(Enumerable.Range('a', 26)
                                   .Concat(Enumerable.Range('0', 10))
                                   .Select(_ => (char)_)
                                   .Concat(new[] { '-', '.', '+' })
                                   .Select(prefix =>
                                   {
                                       var builder = new UriBuilder(packagesUri);
                                       if (builder.Query.Length > 0)
                                           builder.Query += "&";
                                       builder.Query += string.Format("$filter=startswith(Id,'{0}')", prefix);
                                       return builder.Uri;
                                   })
                                   .ToList());

            const int MAX_QUEUE_SIZE = 6;
            IEnumerable<PackageEntry> finalList = new List<PackageEntry>();
            int current_queue_size = 0;
            DateTimeOffset? earliestUpdate = null;
            AutoResetEvent threadComplete = new AutoResetEvent(false);
            Action triggerDownload = null;
            triggerDownload = () =>
            {
                lock (queue)
                {
                    if (current_queue_size >= MAX_QUEUE_SIZE)
                        return;
                    
                    current_queue_size++;
                }
                ThreadPool.QueueUserWorkItem(state =>
                {
                    Uri entry;
                    lock (queue)
                    {
                        if (queue.Count == 0)
                        {
                            current_queue_size--;
                            return;
                        }
                        entry = queue.Dequeue();
                    }
                    DateTimeOffset? feedUpdateTime = null;
                    
                    var newPackages = LoadPackagesFromChainedFeedPages(entry, out feedUpdateTime);
                    if (earliestUpdate == null ||
                        earliestUpdate > feedUpdateTime)
                        earliestUpdate = feedUpdateTime;
                    
                    lock (finalList)
                    {
                        finalList = finalList.Concat(newPackages);
                    }
                    lock (queue)
                    {
                        current_queue_size--;
                        
// ReSharper disable AccessToModifiedClosure
                        
                        triggerDownload();
                        threadComplete.Set();
// ReSharper restore AccessToModifiedClosure
                    }
                });
            };
            // first, try an initial download to check for errors
            try
            {
                finalList = LoadPackagesFromChainedFeedPages(queue.Dequeue(), out earliestUpdate);
            }
            catch
            {
                Mode = NuFeedDownloadMode.Partial;
                return LoadPackagesFromChainedFeedPages(packagesUri, out lastUpdate);
            }
            for (int i = 0; i < MAX_QUEUE_SIZE; i++)
                triggerDownload();
            do
            {
                threadComplete.WaitOne();
            } while (current_queue_size > 0);

            lastUpdate = earliestUpdate;
            return finalList;
        }

        XmlReader GetXml(Uri uri)
        {
            var response = _client.Get(uri).Send();
            if (response.Status.Code / 100 != 2)
                throw new InvalidOperationException(string.Format("The feed at '{0}' responded with status code '{1}', preventing the retrieval of package lists.", uri, response.Status.Code));

            return XmlReader.Create(response.Entity.Stream);
        }

        Func<IPackage> LoadPackage(PackageEntry packageEntry)
        {
            return () =>
            {
                var response = _client.CreateRequest(packageEntry.PackageHref).Get().Send();
                if (response.Entity == null)
                    return null;
                var tempFile = _fileSystem.CreateTempFile();
                var tempDirectory = _fileSystem.CreateTempDirectory();
                using (var tempStream = tempFile.OpenWrite())
                    NuGetConverter.Convert(response.Entity.Stream, tempStream);

                return new CachedZipPackage(this, tempFile, tempDirectory).Load();
            };
        }

        List<PackageEntry> LoadPackagesFromChainedFeedPages(Uri packagesUri, out DateTimeOffset? lastUpdate)
        {
            var feed = NuFeedReader.Read(GetXml(packagesUri));
            lastUpdate = feed.LastUpdate;
            var allPackages = feed.Packages.ToList();
            AtomLink nextAtomLink;
            while ((nextAtomLink = feed.Links["next"].FirstOrDefault()) != null)
            {
                var finalUri = feed.BaseUri.Combine(nextAtomLink);
                feed = NuFeedReader.Read(GetXml(finalUri));

                allPackages.AddRange(feed.Packages);
            }

            return allPackages;
        }


        ILookup<string, IPackageInfo> LoadPackagesThroughCache()
        {
            var existingToken = _cacheManager.GetState(Token);
            if (existingToken.UpdateToken == null)
                AppendPackagesToCache(_packagesUri);
            return _cacheManager.LoadPackages(Token, 
                x => (IPackageInfo)new PackageEntryWrapper(this, x, LoadPackage(x)));
        }
    }

    public class NuFeedToken : UpdateToken
    {
        public NuFeedToken(DateTimeOffset? lastUpdate) : base(lastUpdate.ToString())
        {
        }
    }
}