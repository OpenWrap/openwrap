using System;
using System.Collections.Generic;
using System.IO;
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
    public class NuFeedRepository : IPackageRepository, ISupportAuthentication
    {
        readonly IFileSystem _fileSystem;
        readonly IHttpClient _client;
        readonly Uri _target;
        readonly Uri _packagesUri;
        LazyValue<IEnumerable<IPackageInfo>> _packages;
        NetworkCredential _credentials;

        public NuFeedRepository(IFileSystem fileSystem, IHttpClient client, Uri target, Uri packagesUri)
        {
            _fileSystem = fileSystem;
            _client = client;
            _target = target;
            _packagesUri = packagesUri;
            _packages = Lazy.Is(LoadPackages);
        }
        public string Type { get { return "nufeed"; } }
        IEnumerable<IPackageInfo> LoadPackages()
        {
            var feed = NuFeedReader.Read(GetXml(_packagesUri));

            List<PackageEntry> allPackages = feed.Packages.ToList();
            AtomLink nextAtomLink;
            while((nextAtomLink = feed.Links["next"].FirstOrDefault()) != null)
            {
                var finalUri = feed.BaseUri.Combine(nextAtomLink);
                feed = NuFeedReader.Read(GetXml(finalUri));
                allPackages.AddRange(feed.Packages);
            }
            return allPackages.Select(x => (IPackageInfo)new PackageEntryWrapper(this, x, LoadPackage(x))).ToList();
        }

        XmlReader GetXml(Uri uri)
        {
            var response = _client.Get(uri).Send();
            if (response.Status.Code < 200 || response.Status.Code >= 300)
                throw new InvalidOperationException(string.Format("The feed at '{0}' responded with status code '{1}', preventing the retrieval of package lists.", uri, response.Status.Code));

            return XmlReader.Create(response.Entity.Stream);
        }

        Func<IPackage> LoadPackage(PackageEntry packageEntry)
        {
            return ()=>
            {
                var response = _client.CreateRequest(packageEntry.PackageHref).Get().Send();
                if (response.Entity == null)
                    return null;
                var tempFile = _fileSystem.CreateTempFile();
                var tempDirectory = _fileSystem.CreateTempDirectory();
                using(var tempStream = tempFile.OpenWrite())
                NuGetConverter.Convert(response.Entity.Stream, tempStream);
                
                return new CachedZipPackage(this, tempFile, tempDirectory).Load();
            };
        }

        public ILookup<string, IPackageInfo> PackagesByName
        {
            get { return _packages.Value.ToLookup(x => x.Name, StringComparer.OrdinalIgnoreCase); }
        }

        public IEnumerable<IPackageInfo> FindAll(PackageDependency dependency)
        {
            return PackagesByName.FindAll(dependency);
        }

        public void RefreshPackages()
        {
            _packages = Lazy.Is(LoadPackages);
        }

        public string Name
        {
            get { return "NuGet OData feed"; }
        }

        public string Token
        {
            get { return string.Format("[nuget][{0}]{1}", _target, _packagesUri); }
        }

        public TFeature Feature<TFeature>() where TFeature : class, IRepositoryFeature
        {
            return this as TFeature;
        }

        public IDisposable WithCredentials(NetworkCredential credentials)
        {
            var oldCredentials = _credentials;
            _credentials = credentials;
            return new ActionOnDispose(() => _credentials = oldCredentials);
        }
    }
}