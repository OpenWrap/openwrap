using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using OpenWrap.Exports;
using OpenWrap.Dependencies;
using OpenWrap.IO;

namespace OpenWrap.Repositories.Http
{
    public class HttpRepository : IPackageRepository
    {
        readonly IHttpRepositoryNavigator _navigator;

        public HttpRepository(IFileSystem fileSystem, IHttpRepositoryNavigator navigator)
        {
            _navigator = navigator;
            PackagesByName = (from package in navigator.Index().Packages
                              select new HttpPackageInfo(fileSystem, this, navigator, package))
                .Cast<IPackageInfo>().ToLookup(x => x.Name);
        }

        DateTime? GetModifiedTimeUtc(XAttribute attribute)
        {
            if (attribute == null) return null;
            DateTime dt;
            return !DateTime.TryParse(attribute.Value, out dt) ? (DateTime?)null : dt;
        }

        public ILookup<string, IPackageInfo> PackagesByName { get; private set; }

        public IPackageInfo Find(WrapDependency dependency)
        {
            return PackagesByName.Find(dependency);
        }

        public IPackageInfo Publish(string packageFileName, Stream packageStream)
        {
            throw new NotImplementedException();
        }

        public bool CanPublish
        {
            get { return false; }
        }

        public string Name
        {
            get { return string.Format("Remote [{0}]", _navigator); }
        }
    }
}