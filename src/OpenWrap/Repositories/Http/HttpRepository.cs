using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using OpenWrap.Dependencies;
using OpenFileSystem.IO;

namespace OpenWrap.Repositories.Http
{
    public class HttpRepository : IPackageRepository, ISupportPublishing
    {
        readonly IHttpRepositoryNavigator _navigator;
        readonly IFileSystem _fileSystem;
        readonly IEnumerable<HttpPackageInfo> _packagesQuery;
        ILookup<string, IPackageInfo> _packagesByName;

        public HttpRepository(IFileSystem fileSystem, string repositoryName, IHttpRepositoryNavigator navigator)
        {
            _navigator = navigator;
            _fileSystem = fileSystem;
            Name = repositoryName;
            _packagesQuery = LoadPackages(navigator, fileSystem);
        }

        public IEnumerable<IPackageInfo> FindAll(PackageDependency dependency)
        {
            return PackagesByName.FindAll(dependency);
        }

        public void Refresh()
        {
            _packagesByName = null;
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

        public IPackageInfo Find(PackageDependency dependency)
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
            return PackagesByName[PackageNameUtility.GetName(packageFileName)].FirstOrDefault(x => x.Version == PackageNameUtility.GetVersion(packageFileName));
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
                    _packagesByName = _packagesByName ?? Enumerable.Empty<IPackageInfo>().ToLookup(x=>string.Empty);
                }
            }
        }

        DateTime? GetModifiedTimeUtc(XAttribute attribute)
        {
            if (attribute == null) return null;
            DateTime dt;
            return !DateTime.TryParse(attribute.Value, out dt) ? (DateTime?)null : dt;
        }

        public bool CanDelete { get { return false; } }

        public void Delete(IPackageInfo package) { }
    }
}
