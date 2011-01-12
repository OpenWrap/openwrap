using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenFileSystem.IO;
using OpenWrap.PackageManagement.Packages;
using OpenWrap.PackageModel;
using OpenWrap.Runtime;

namespace OpenWrap.Repositories
{
    public class CurrentDirectoryRepository : IPackageRepository
    {
        ILookup<string, IPackageInfo> _packages;

        public bool CanPublish
        {
            get { return false; }
        }

        public string Name
        {
            get { return "Current directory"; }
        }

        public ILookup<string, IPackageInfo> PackagesByName
        {
            get
            {
                if (_packages == null)
                    _packages = (from wrapFile in Environment.CurrentDirectory.Files("*.wrap")
                                 let tempFolder = FileSystem.GetTempDirectory().GetDirectory(Guid.NewGuid().ToString())
                                 select (IPackageInfo)new CachedZipPackage(this, wrapFile, tempFolder, ExportBuilders.All))
                            .ToLookup(x => x.Name, StringComparer.OrdinalIgnoreCase);
                return _packages;
            }
        }

        protected IEnvironment Environment
        {
            get { return Services.Services.GetService<IEnvironment>(); }
        }

        protected IFileSystem FileSystem
        {
            get { return Services.Services.GetService<IFileSystem>(); }
        }

        public IPackageInfo Publish(string packageFileName, Stream packageStream)
        {
            throw new NotSupportedException();
        }

        public IEnumerable<IPackageInfo> FindAll(PackageDependency dependency)
        {
            return PackagesByName.FindAll(dependency);
        }

        public void RefreshPackages()
        {
            _packages = null;
        }
    }
}