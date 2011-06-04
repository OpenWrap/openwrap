using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using OpenRasta.Client;
using OpenWrap.PackageModel;
using OpenWrap.Repositories.Http;

namespace OpenWrap.Repositories.NuFeed
{
    public class NuFeedRepository : IPackageRepository
    {
        readonly IHttpClient _client;
        readonly Uri _target;
        readonly Uri _packagesUri;
        LazyValue<IEnumerable<IPackageInfo>> _packages;

        public NuFeedRepository(IHttpClient client, Uri target, Uri packagesUri)
        {
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
                throw new NotImplementedException();
            };
        }

        public ILookup<string, IPackageInfo> PackagesByName
        {
            get { return _packages.Value.ToLookup(x => x.Name); }
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
    }
}