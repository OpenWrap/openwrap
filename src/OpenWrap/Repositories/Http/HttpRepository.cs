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

        public HttpRepository(IFileSystem fileSystem, IHttpRepositoryNavigator navigator)
        {
            _navigator = navigator;
            IndexDocument = navigator.Index();
            _packagesQuery = IndexDocument == null
                                     ? Enumerable.Empty<HttpPackageInfo>()
                                     : (
                                               from package in IndexDocument.Packages
                                               select new HttpPackageInfo(fileSystem, this, navigator, package)
                                       );
        }

        public bool CanPublish
        {
            get { return Navigator.CanPublish; }
        }

        public PackageDocument IndexDocument { get; private set; }

        public string Name
        {
            get { return string.Format("Remote [{0}]", Navigator); }
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