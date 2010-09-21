using System;
using System.IO;
using System.Linq;
using OpenWrap.Dependencies;
using OpenFileSystem.IO;
using OpenWrap.Services;

namespace OpenWrap.Repositories
{
    public class CurrentDirectoryRepository : IPackageRepository
    {
        protected IFileSystem FileSystem {get{ return WrapServices.GetService<IFileSystem>();}}
        protected IEnvironment Environment { get{ return WrapServices.GetService<IEnvironment>();}}

        public ILookup<string, IPackageInfo> PackagesByName
        {
            get
            {
                return (from wrapFile in Environment.CurrentDirectory.Files("*.wrap")
                        let tempFolder = FileSystem.CreateTempDirectory()
                        select (IPackageInfo)new CachedZipPackage(this, wrapFile, tempFolder, ExportBuilders.All, false))
                    .ToLookup(x=>x.Name);
            }
        }

        public IPackageInfo Find(WrapDependency dependency)
        {
            return PackagesByName.Find(dependency);
        }

        public IPackageInfo Publish(string packageFileName, Stream packageStream)
        {
            throw new NotSupportedException();
        }

        public bool CanPublish
        {
            get { return false; }
        }

        public void Refresh()
        {
        }

        public string Name
        {
            get { return "Current directory"; }
        }
    }
}