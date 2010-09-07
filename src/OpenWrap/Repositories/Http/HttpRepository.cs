using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using OpenWrap.Exports;
using OpenWrap.Dependencies;
using OpenFileSystem.IO;
using OpenWrap.Tasks;

namespace OpenWrap.Repositories.Http
{
    public class HttpRepository : IPackageRepository
    {
        readonly IHttpRepositoryNavigator _navigator;


        public HttpRepository(IFileSystem fileSystem, IHttpRepositoryNavigator navigator)
        {
            _navigator = navigator;
            var index = navigator.Index();
            _packagesQuery = index == null
                                     ? Enumerable.Empty<HttpPackageInfo>()
                                     : (
                                            from package in index.Packages
                                            select new HttpPackageInfo(fileSystem, this, navigator, package)
                                       );
        }

        DateTime? GetModifiedTimeUtc(XAttribute attribute)
        {
            if (attribute == null) return null;
            DateTime dt;
            return !DateTime.TryParse(attribute.Value, out dt) ? (DateTime?)null : dt;
        }

        ILookup<string, IPackageInfo> _packagesByName;
        IEnumerable<HttpPackageInfo> _packagesQuery;

        public ILookup<string, IPackageInfo> PackagesByName
        {
            get
            {
                EnsureDataLoaded();
                return _packagesByName;
            }
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
            }
        }

        public IPackageInfo Find(WrapDependency dependency)
        {
            return PackagesByName.Find(dependency);
        }

        public IPackageInfo Publish(string packageFileName, Stream packageStream)
        {
            if (!_navigator.CanPublish)
                throw new InvalidOperationException(string.Format("The repository {0} is read-only.", _navigator));

            _navigator.PushPackage(packageFileName, packageStream);
            _packagesByName = null;
            EnsureDataLoaded();
            return PackagesByName[WrapNameUtility.GetName(packageFileName)].FirstOrDefault(x=>x.Version == WrapNameUtility.GetVersion(packageFileName));
        }

        public bool CanPublish
        {
            get { return _navigator.CanPublish; }
        }

        public string Name
        {
            get { return string.Format("Remote [{0}]", _navigator); }
        }
    }
}