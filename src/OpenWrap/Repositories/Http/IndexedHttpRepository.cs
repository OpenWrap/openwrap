using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.InMemory;
using OpenWrap.PackageManagement.Packages;
using OpenWrap.PackageModel;

namespace OpenWrap.Repositories.Http
{
    public class IndexedHttpRepository : IPackageRepository, ISupportAuthentication, ISupportPublishing
    {
        readonly IHttpRepositoryNavigator _navigator;
        readonly IEnumerable<HttpPackageInfo> _packagesQuery;
        ILookup<string, IPackageInfo> _packagesByName;

        public IndexedHttpRepository(IFileSystem fileSystem, string repositoryName, IHttpRepositoryNavigator navigator)
        {
            _navigator = navigator;
            Name = repositoryName;
            _packagesQuery = LoadPackages(navigator, fileSystem);
        }

        public PackageFeed IndexFeed { get; private set; }

        public string Name { get; private set; }

        public IHttpRepositoryNavigator Navigator
        {
            get { return _navigator; }
        }

        public ILookup<string, IPackageInfo> PackagesByName
        {
            get
            {
                EnsureDataLoaded();
                return _packagesByName;
            }
        }

        public virtual string Token { get; set; }

        public virtual string Type
        {
            get { return "indexed-http"; }
        }

        public override string ToString()
        {
            return string.Format("Remote {0} [{1}]", Name, Navigator);
        }

        public TFeature Feature<TFeature>() where TFeature : class, IRepositoryFeature
        {
            if (typeof(TFeature) == typeof(ISupportPublishing) && _navigator.CanPublish == false) return null;
            return this as TFeature;
        }

        public void RefreshPackages()
        {
            _packagesByName = null;
        }

        IDisposable ISupportAuthentication.WithCredentials(NetworkCredential credentials)
        {
            var auth = _navigator as ISupportAuthentication;
            var initial = CurrentCredentials;
            CurrentCredentials = credentials;
            return auth == null
                       ? new ActionOnDispose(() => { CurrentCredentials = initial; })
                       : auth.WithCredentials(credentials);
        }

        public NetworkCredential CurrentCredentials { get; private set; }

        public IPackagePublisher Publisher()
        {
            return new PackagePublisher(Publish);
        }

        void EnsureDataLoaded()
        {
            if (_packagesByName == null)
            {
                try
                {
                    _packagesByName = _packagesQuery
                        .Cast<IPackageInfo>()
                        .ToLookup(x => x.Name, StringComparer.OrdinalIgnoreCase);
                }
                catch
                {
                }
                finally
                {
                    _packagesByName = _packagesByName ?? Enumerable.Empty<IPackageInfo>().ToLookup(x => string.Empty);
                }
            }
        }

        IEnumerable<HttpPackageInfo> LoadPackages(IHttpRepositoryNavigator navigator, IFileSystem fileSystem)
        {
            IndexFeed = navigator.Index();

            if (IndexFeed == null)
                yield break;
            foreach (var package in IndexFeed.Packages)
                yield return new HttpPackageInfo(fileSystem, this, navigator, package);
        }

        void Publish(IPackageRepository packageRepository, string packageFileName, Stream packageStream)
        {
            if (!Navigator.CanPublish)
                throw new InvalidOperationException(string.Format("The repository {0} is read-only.", Navigator));

            Navigator.PushPackage(packageFileName, packageStream);
            _packagesByName = null;

            EnsureDataLoaded();
        }
    }
}