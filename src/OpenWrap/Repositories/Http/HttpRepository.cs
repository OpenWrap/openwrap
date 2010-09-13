using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using OpenWrap.Dependencies;
using OpenFileSystem.IO;

namespace OpenWrap.Repositories.Http
{
    public class HttpRepository : IPackageRepository
    {
        readonly IHttpRepositoryNavigator _navigator;


        readonly IEnumerable<HttpPackageInfo> _packagesQuery;
        ILookup<string, IPackageInfo> _packagesByName;

        public HttpRepository(IFileSystem fileSystem, string repositoryName, IHttpRepositoryNavigator navigator)
        {
            _navigator = navigator;
            Name = repositoryName;
            _packagesQuery = LoadPackages(navigator, fileSystem);
        }

        IEnumerable<HttpPackageInfo> LoadPackages(IHttpRepositoryNavigator navigator, IFileSystem fileSystem)
        {
            IndexDocument = navigator.Index();

            if (IndexDocument == null)
                yield break;
            foreach (var package in IndexDocument.Packages)
                yield return new HttpPackageInfo(fileSystem, this, navigator, package);
        }

        public bool CanPublish
        {
            get { return Navigator.CanPublish; }
        }

        public PackageDocument IndexDocument { get; private set; }

        public string Name { get; private set; }
        public override string ToString()
        {
            return string.Format("Remote {0} [{1}]", Name, Navigator);
        }
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

        public IPackageInfo Find(WrapDependency dependency)
        {
            return PackagesByName.Find(dependency);
        }

        public IPackageInfo Publish(string packageFileName, Stream packageStream)
        {
            if (!Navigator.CanPublish)
                throw new InvalidOperationException(string.Format("The repository {0} is read-only.", Navigator));

            Navigator.PushPackage(packageFileName, packageStream);
            _packagesByName = null;
            EnsureDataLoaded();
            return PackagesByName[WrapNameUtility.GetName(packageFileName)].FirstOrDefault(x => x.Version == WrapNameUtility.GetVersion(packageFileName));
        }

        void EnsureDataLoaded()
        {
            if (_packagesByName == null)
            {
                try
                {
                    _packagesByName = _packagesQuery.Cast<IPackageInfo>().ToLookup(x => x.Name);
                }
                catch
                {
                }
                finally
                {
                    _packagesByName = _packagesByName ?? Enumerable.Empty<IPackageInfo>().ToLookup(x=>string.Empty  );
                }
            }
        }

        DateTime? GetModifiedTimeUtc(XAttribute attribute)
        {
            if (attribute == null) return null;
            DateTime dt;
            return !DateTime.TryParse(attribute.Value, out dt) ? (DateTime?)null : dt;
        }
    }
}