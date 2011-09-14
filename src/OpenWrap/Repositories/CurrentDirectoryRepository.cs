using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenFileSystem.IO;
using OpenWrap.PackageManagement.Packages;
using OpenWrap.PackageModel;
using OpenWrap.Runtime;
using OpenWrap.Services;

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

        public string Token
        {
            get { return "[directory]" + Environment.CurrentDirectory.Path.FullPath; }
        }
        public string Type
        {
            get { return "Current Directory"; }
        }

        public TFeature Feature<TFeature>() where TFeature : class, IRepositoryFeature
        {
            return this as TFeature;
        }

        public ILookup<string, IPackageInfo> PackagesByName
        {
            get
            {
                if (_packages == null)
                    _packages = (from wrapFile in Environment.CurrentDirectory.Files("*.wrap")
                                 let tempFolder = FileSystem.GetTempDirectory().GetDirectory(Guid.NewGuid().ToString())
                                 let package = (IPackageInfo)new CachedZipPackage(this, wrapFile, tempFolder)
                                 where package.IsValid
                                 select package)
                            .ToLookup(x => x.Name, StringComparer.OrdinalIgnoreCase);
                return _packages;
            }
        }

        protected IEnvironment Environment
        {
            get { return ServiceLocator.GetService<IEnvironment>(); }
        }

        protected IFileSystem FileSystem
        {
            get { return ServiceLocator.GetService<IFileSystem>(); }
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