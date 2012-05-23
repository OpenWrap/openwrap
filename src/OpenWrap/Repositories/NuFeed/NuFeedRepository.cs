using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml;
using OpenFileSystem.IO;
using OpenRasta.Client;
using OpenWrap.PackageManagement.Packages;
using OpenWrap.PackageModel;
using OpenWrap.Repositories.Http;
using OpenWrap.Repositories.NuGet;

namespace OpenWrap.Repositories.NuFeed
{
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

            RefreshPackages();
        }

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
                                          : LoadPackagesFromFeedPages(_packagesUri, out lastUpdated)
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
                updateUri.Query = string.Format("LastUpdated gt datetime'{0}'", token.Value);
                AppendPackagesToCache(updateUri.Uri);
            }
            RefreshPackages();
        }

        void AppendPackagesToCache(Uri packagesUri)
        {
            DateTimeOffset? lastUpdate;
            var packages = LoadPackagesFromFeedPages(packagesUri, out lastUpdate);
            var updateToken = new NuFeedToken(lastUpdate);
            _cacheManager.AppendPackages(Token, updateToken, packages);
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

        List<PackageEntry> LoadPackagesFromFeedPages(Uri packagesUri, out DateTimeOffset? lastUpdate)
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
            return _cacheManager.LoadPackages(Token, x => (IPackageInfo)new PackageEntryWrapper(this, x, LoadPackage(x)));
        }
    }

    public class NuFeedToken : UpdateToken
    {
        public NuFeedToken(DateTimeOffset? lastUpdate) : base(lastUpdate.ToString())
        {
        }
    }
}