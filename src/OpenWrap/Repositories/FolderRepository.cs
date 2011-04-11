using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenFileSystem.IO;
using OpenWrap.IO;
using OpenWrap.PackageManagement;
using OpenWrap.PackageManagement.Packages;
using OpenWrap.PackageModel;
using StreamExtensions = OpenWrap.IO.StreamExtensions;

namespace OpenWrap.Repositories
{
    /// <summary>
    ///   Provides a repository that can read packages from a directory using the default structure.
    /// </summary>
    public class FolderRepository : ISupportCleaning, ISupportPublishing, ISupportAnchoring
    {
        readonly bool _anchoringEnabled;
        readonly IDirectory _rootCacheDirectory;
        readonly bool _useSymLinks;

        public FolderRepository(IDirectory packageBasePath, FolderRepositoryOptions options = FolderRepositoryOptions.Default)
        {
            _useSymLinks = (options & FolderRepositoryOptions.UseSymLinks) == FolderRepositoryOptions.UseSymLinks;
            _anchoringEnabled = (options & FolderRepositoryOptions.AnchoringEnabled) == FolderRepositoryOptions.AnchoringEnabled;
            if (packageBasePath == null) throw new ArgumentNullException("packageBasePath");

            BasePath = packageBasePath;


            _rootCacheDirectory = BasePath.GetOrCreateDirectory("_cache");
            RefreshPackages();
        }

        public IDirectory BasePath { get; set; }
        public string Name { get; set; }

        public ILookup<string, IPackageInfo> PackagesByName
        {
            get { return Packages.Select(x => x.Package).ToLookup(x => x.Name, StringComparer.OrdinalIgnoreCase); }
        }

        protected List<PackageLocation> Packages { get; set; }

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

        public IEnumerable<PackageAnchoredResult> AnchorPackages(IEnumerable<IPackageInfo> packagesToAnchor)
        {
            if (!_anchoringEnabled)
                yield break;
            AnchorageStrategy anchorageStrategy = _useSymLinks ? (AnchorageStrategy)new SymLinkAnchorageStrategy() : new DirectoryCopyAnchorageStrategy();


            foreach (var package in packagesToAnchor)
            {
                if (package.Source != this)
                    continue;
                if (package.Load() == null)
                    yield return new PackageAnchoredResult(package.Source as ISupportAnchoring, package, false);

                var success = anchorageStrategy.Anchor(BasePath, Packages.First(x => x.Package == package).CacheDirectory, package.Name);
                if (success != null)
                    yield return new PackageAnchoredResult(this, package, (bool)success);
            }
        }

        public abstract class AnchorageStrategy
        {
            public abstract bool? Anchor(IDirectory packagesDirectory, IDirectory packageDirectory, string anchorName);
        }
        class DirectoryCopyAnchorageStrategy : AnchorageStrategy
        {
            public override bool? Anchor(IDirectory packagesDirectory, IDirectory packageDirectory, string anchorName)
            {
                var anchoredDirectory = packagesDirectory.GetDirectory(anchorName);

                if (anchoredDirectory.Exists && anchoredDirectory.IsHardLink && !SafeDelete(anchoredDirectory))
                    return false;
                anchoredDirectory = packagesDirectory.GetDirectory(anchorName);
                if (anchoredDirectory.Exists)
                {
                    var anchorFile = anchoredDirectory.GetFile(".anchored");
                    if (anchorFile.Exists && anchorFile.ReadString() == packageDirectory.Name) return null;

                    if (!SafeDelete(anchoredDirectory)) return false;
                }
                packageDirectory.CopyTo(anchoredDirectory);
                anchoredDirectory.GetFile(".anchored").WriteString(packageDirectory.Name);
                return true;
            }
        }
        public class SymLinkAnchorageStrategy : AnchorageStrategy
        {
            public override bool? Anchor(IDirectory packagesDirectory, IDirectory packageDirectory, string anchorName)
            {
                var anchoredDirectory = packagesDirectory.GetDirectory(anchorName);
             
                
                if (anchoredDirectory.Exists)
                {
                    if (anchoredDirectory.IsHardLink && anchoredDirectory.Target.Equals(packageDirectory))
                        return null;
                    if (!SafeDelete(anchoredDirectory)) return false;
                }
                packageDirectory.LinkTo(anchoredDirectory.Path.FullPath);
                return true;
            }
        }
        static bool SafeDelete(IDirectory directory)
        {
            try
            {
                int count = 0;
                IDirectory deleteableDirectory;
                do
                {
                    deleteableDirectory = directory.Parent.GetDirectory("_" + directory.Name + "." + count++ + ".deleteme");
                } while (deleteableDirectory.Exists);

                directory.MoveTo(deleteableDirectory);
                deleteableDirectory.Delete();
            }
            catch (IOException)
            {
                return false;
            }
            return true;
        }
        public IEnumerable<PackageCleanResult> Clean(IEnumerable<IPackageInfo> packagesToKeep)
        {
            packagesToKeep = packagesToKeep.ToList();
            var packagesToRemove = Packages.Where(x => !packagesToKeep.Contains(x.Package)).ToList();
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
            }
        }

        public IPackageInfo Publish(string packageFileName, Stream packageStream)
        {
            packageFileName = PackageNameUtility.NormalizeFileName(packageFileName);

            var wrapFile = BasePath.GetFile(packageFileName);
            if (wrapFile.Exists)
                return null;

            using (var file = wrapFile.OpenWrite())
                StreamExtensions.CopyTo(packageStream, file);

            var newPackageCacheDir = _rootCacheDirectory.GetDirectory(wrapFile.NameWithoutExtension);
            var newPackage = new CachedZipPackage(this, wrapFile, newPackageCacheDir);
            Packages.Add(new PackageLocation(newPackageCacheDir, newPackage));
            return newPackage;
        }
        public IPackagePublisher Publisher()
        {
            return new PackagePublisher(Publish, PublishCompleted);
        }
        void PublishCompleted()
        {
            foreach (var package in Packages) package.Package.Load();
            RefreshPackages();
        }


        IPackageInfo CreatePackageInstance(IDirectory cacheDirectory, IFile wrapFile)
        {
            if (cacheDirectory.Exists)
                return new UncompressedPackage(this, wrapFile, cacheDirectory);
            return new CachedZipPackage(this, wrapFile, cacheDirectory);
        }

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

    }
}