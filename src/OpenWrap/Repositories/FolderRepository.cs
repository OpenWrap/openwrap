using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenWrap.Dependencies;
using OpenFileSystem.IO;

namespace OpenWrap.Repositories
{
    /// <summary>
    /// Provides a repository that can read packages from a directory using the default structure.
    /// </summary>
    public class FolderRepository : ISupportCleaning, ISupportPublishing, ISupportAnchoring
    {
        IDirectory _rootCacheDirectory;

        public FolderRepository(IDirectory packageBasePath)
        {
            if (packageBasePath == null) throw new ArgumentNullException("packageBasePath");

            BasePath = packageBasePath;


            _rootCacheDirectory = BasePath.GetOrCreateDirectory("_cache");
            RefreshPackages();

        }

        public bool EnableAnchoring { get; set; }
        public IEnumerable<IPackageInfo> FindAll(PackageDependency dependency)
        {
            return PackagesByName.FindAll(dependency);
        }

        public void RefreshPackages()
        {
            Packages = (from wrapFile in BasePath.Files("*.wrap")
                        let packageFullName = wrapFile.NameWithoutExtension
                        let packageVersion = PackageNameUtility.GetVersion(packageFullName)
                        where packageVersion != null
                        let packageCacheDirectory = _rootCacheDirectory.GetDirectory(packageFullName)
                        select new PackageLocation(
                            packageCacheDirectory,
                            CreatePackageInstance(packageCacheDirectory, wrapFile)
                        )).ToList();
        }

        IPackageInfo CreatePackageInstance(IDirectory cacheDirectory, IFile wrapFile)
        {
            if (cacheDirectory.Exists)
                return new UncompressedPackage(this, wrapFile, cacheDirectory, ExportBuilders.All);
            return new CachedZipPackage(this, wrapFile, cacheDirectory, ExportBuilders.All);
        }

        public IDirectory BasePath { get; set; }

        protected class PackageLocation
        {
            public PackageLocation(IDirectory cacheDir, IPackageInfo package)
            {
                CacheDirectory = cacheDir;
                Package = package;
            }

            public IDirectory CacheDirectory { get; set; }
            public IPackageInfo Package { get; set; }
        }
        public ILookup<string, IPackageInfo> PackagesByName
        {
            get { return Packages.Select(x => x.Package).ToLookup(x => x.Name, StringComparer.OrdinalIgnoreCase); }
        }

        protected List<PackageLocation> Packages { get; set; }

        public IPackageInfo Find(PackageDependency dependency)
        {
            return PackagesByName.Find(dependency);
        }

        public IPackageInfo Publish(string packageFileName, Stream packageStream)
        {
            packageFileName = PackageNameUtility.NormalizeFileName(packageFileName);

            var wrapFile = BasePath.GetFile(packageFileName);
            if (wrapFile.Exists)
                return null;

            using (var file = wrapFile.OpenWrite())
                packageStream.CopyTo(file);

            var newPackageCacheDir = _rootCacheDirectory.GetDirectory(wrapFile.NameWithoutExtension);
            var newPackage = new CachedZipPackage(this, wrapFile, newPackageCacheDir, ExportBuilders.All);
            Packages.Add(new PackageLocation(newPackageCacheDir, newPackage));
            return newPackage;
        }

        public void PublishCompleted()
        {
            foreach (var package in Packages) package.Package.Load();
            RefreshPackages();
        }
        
        public string Name
        {
            get;
            set;
        }

        public IEnumerable<IPackageInfo> AnchorPackages(IEnumerable<IPackageInfo> packagesToAnchor)
        {
            if (!EnableAnchoring)
                return Enumerable.Empty<IPackageInfo>();

            List<IPackageInfo> failed = new List<IPackageInfo>();
            foreach (var package in packagesToAnchor)
            {
                if (package.Source != this)
                    continue;
                package.Load();
                var anchoredDirectory = BasePath.GetDirectory(package.Name);
                var packageDirectory = Packages.First(x => x.Package == package).CacheDirectory;
                if (anchoredDirectory.Exists)
                {
                    if (anchoredDirectory.IsHardLink && anchoredDirectory.Target.Equals(packageDirectory))
                        continue;
                    bool success = true;
                    var temporaryDirectoryPath = anchoredDirectory.Parent.Path.Combine(anchoredDirectory.Name + ".old").FullPath;
                    try
                    {
                        System.IO.Directory.Move(anchoredDirectory.Path.FullPath, temporaryDirectoryPath);
                        var anchoredPath = anchoredDirectory.Path;
                        packageDirectory.LinkTo(anchoredPath.FullPath);

                    }
                    catch (Exception)
                    {
                        failed.Add(package);
                        success = false;
                    }
                    if (success)
                    {
                        try
                        {
                            anchoredDirectory.FileSystem.GetDirectory(temporaryDirectoryPath).Delete();
                        }
                        catch (Exception)
                        {
                            failed.Add(package);
                        }
                    }
                }
                else
                {
                    packageDirectory.LinkTo(anchoredDirectory.Path.FullPath);
                }
            }
            return failed;
        }

        public IEnumerable<PackageCleanResult> Clean(IEnumerable<IPackageInfo> packagesToKeep)
        {
            packagesToKeep = packagesToKeep.ToList();
            var packagesToRemove = Packages.Where(x => !packagesToKeep.Contains(x.Package)).ToList();
            bool somethingDone = false;
            foreach (var packageInfo in packagesToRemove)
            {
                if (!Packages.Contains(packageInfo))
                    throw new ArgumentException("Supplied packageInfo must belong to the FolderRepository.", "packageInfo");

                if (packageInfo.CacheDirectory.TryDelete())
                {
                    Packages.Remove(packageInfo);

                    BasePath.GetFile(packageInfo.Package.FullName + ".wrap").Delete();
                    yield return new PackageCleanResult(packageInfo.Package, true);
                }
                else
                {
                    yield return new PackageCleanResult(packageInfo.Package, false);
                }
                somethingDone = true;
            }
        }

    }
}