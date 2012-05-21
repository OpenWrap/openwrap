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
        readonly IHttpClient _client;
        readonly IFileSystem _fileSystem;
        readonly Uri _packagesUri;
        readonly Uri _target;
        LazyValue<ILookup<string,IPackageInfo>> _packages;
        PackageCacheManager _cacheManager;

        public NuFeedRepository(IFileSystem fileSystem, PackageCacheManager cacheManager, IHttpClient client, Uri target, Uri packagesUri)
        {
            _fileSystem = fileSystem;
            _client = client;
            _target = target;
            _packagesUri = packagesUri;
            _cacheManager = cacheManager;
            RefreshPackages();
        }

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
            return this as TFeature;
        }

        public void RefreshPackages()
        {
            _packages = Lazy.Is(() =>
            {
                var existingToken = _cacheManager.GetState(Token);
                if (existingToken == null)
                {
                    AppendPackages(_packagesUri);
                }
                return _cacheManager.LoadPackages(Token, x => (IPackageInfo)new PackageEntryWrapper(this, x, LoadPackage(x)));
            });
        }

        void AppendPackages(Uri packagesUri)
        {
            DateTimeOffset? lastUpdate;
            var packages = LoadPackages(packagesUri, out lastUpdate);
            var updateToken = new NuFeedToken(lastUpdate);
            _cacheManager.AppendPackages(Token, updateToken, packages);
        }

        public IDisposable WithCredentials(NetworkCredential credentials)
        {
            var oldCredentials = CurrentCredentials;
            CurrentCredentials = credentials;
            return new ActionOnDispose(() => CurrentCredentials = oldCredentials);
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

        List<PackageEntry> LoadPackages(Uri packagesUri, out DateTimeOffset? lastUpdate)
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

        public CacheState GetState()
        {
            return _cacheManager.GetState(Token);
        }

        public void Update()
        {

            var existingToken = _cacheManager.GetState(Token);
            
            if (existingToken.UpdateToken == null)
            {
                AppendPackages(_packagesUri);
            }
            else
            {
                var token = existingToken.UpdateToken;
                var updateUri = new UriBuilder(_packagesUri);
                updateUri.Query = string.Format("LastUpdated gt datetime'{0}'", token.Value);
                AppendPackages(updateUri.Uri);
            }
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }
    }

    public class NuFeedToken : UpdateToken
    {
        public NuFeedToken(DateTimeOffset? lastUpdate) : base(lastUpdate.ToString())
        {
        }
    }
}