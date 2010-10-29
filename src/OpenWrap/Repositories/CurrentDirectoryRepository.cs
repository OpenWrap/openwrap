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
        protected IFileSystem FileSystem {get{ return Services.Services.GetService<IFileSystem>();}}
        protected IEnvironment Environment { get{ return Services.Services.GetService<IEnvironment>();}}
        ILookup<string, IPackageInfo> _packages;

        public ILookup<string, IPackageInfo> PackagesByName
        {
            get
            {
                if (_packages == null)
                _packages = (from wrapFile in Environment.CurrentDirectory.Files("*.wrap")
                        let tempFolder = FileSystem.GetTempDirectory().GetDirectory(Guid.NewGuid().ToString())
                        select (IPackageInfo)new CachedZipPackage(this, wrapFile, tempFolder, ExportBuilders.All))
                    .ToLookup(x=>x.Name, StringComparer.OrdinalIgnoreCase);
                return _packages;
            }
        }

        public IPackageInfo Find(PackageDependency dependency)
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
            _packages = null;
        }

        public string Name
        {
            get { return "Current directory"; }
        }

        public bool CanDelete { get { return false; } }

        public void Delete(IPackageInfo package) { }
    }
}