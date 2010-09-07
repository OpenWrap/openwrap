using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using OpenWrap.Dependencies;

namespace OpenWrap.Repositories
{
    public class IndexedFolderRepository : IPackageRepository
    {
        public IndexedFolderRepository(string path)
        {
            throw new NotImplementedException();
        }

        public ILookup<string, IPackageInfo> PackagesByName
        {
            get { throw new NotImplementedException(); }
        }

        public IPackageInfo Find(WrapDependency dependency)
        {
            throw new NotImplementedException();
        }

        public IPackageInfo Publish(string packageFileName, Stream packageStream)
        {
            throw new NotImplementedException();
        }

        public bool CanPublish
        {
            get { throw new NotImplementedException(); }
        }

        public string Name
        {
            get { throw new NotImplementedException(); }
        }

        public XDocument IndexFile { get; set; }
    }
}