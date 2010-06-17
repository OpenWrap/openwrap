using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenWrap.Dependencies;
using OpenWrap.IO;

namespace OpenWrap.Repositories
{
    /// <summary>
    /// Provides a repository that can read packages from a directory using the default structure.
    /// </summary>
    public class FolderRepository : IPackageRepository
    {
        IDirectory _rootCacheDirectory;

        public FolderRepository(IDirectory packageBasePath)
        {
            if (packageBasePath == null) throw new ArgumentNullException("packageBasePath");

            BasePath = packageBasePath;

            _rootCacheDirectory = BasePath.GetOrCreateDirectory("cache");
            Packages = (from wrapFile in BasePath.Files("*.wrap")
                        let wrapName = wrapFile.NameWithoutExtension
                        let packageVersion = WrapNameUtility.GetVersion(wrapName)
                        where packageVersion != null
                        let cacheDirectory = _rootCacheDirectory.GetDirectory(wrapName)
                        select cacheDirectory.Exists
                                   ? (IPackageInfo)new UncompressedPackage(this, wrapFile, cacheDirectory, ExportBuilders.All)
                                   : (IPackageInfo)new ZipPackage(this, wrapFile, cacheDirectory, ExportBuilders.All)).ToList();
        }

        public IDirectory BasePath { get; set; }

        public ILookup<string, IPackageInfo> PackagesByName
        {
            get { return Packages.ToLookup(x => x.Name); }
        }

        protected List<IPackageInfo> Packages { get; set; }

        public IPackageInfo Find(WrapDependency dependency)
        {
            return PackagesByName.Find(dependency);
        }

        public IPackageInfo Publish(string packageFileName, Stream packageStream)
        {
            var wrapFile = BasePath.GetFile(packageFileName + ".wrap");
            if (wrapFile.Exists)
                return null;

            using (var file = wrapFile.OpenWrite())
                packageStream.CopyTo(file);

            var newPackage = new ZipPackage(this, wrapFile, _rootCacheDirectory.GetDirectory(wrapFile.NameWithoutExtension), ExportBuilders.All);
            Packages.Add(newPackage);
            return newPackage;
        }

        public bool CanPublish
        {
            get { return true; }
        }
    }
}