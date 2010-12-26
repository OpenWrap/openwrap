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

        public IPackageInfo Find(PackageDependency dependency)
        {
            return PackagesByName.Find(dependency);
        }

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

            var failed = new List<IPackageInfo>();
            foreach (var package in packagesToAnchor)
            {
                if (package.Source != this)
                    continue;
                package.Load();
                var anchoredDirectory = BasePath.GetDirectory(package.Name);
                var packageDirectory = Packages.First(x => x.Package == package).CacheDirectory;
                if (anchoredDirectory.Exists)
                {
                    if (_useSymLinks)
                    {
                        if (anchoredDirectory.IsHardLink && anchoredDirectory.Target.Equals(packageDirectory))
                            continue;
                    }
                    else
                    {
                        var anchorFile = anchoredDirectory.GetFile(".anchored");
                        if (anchorFile.Exists)
                        {
                            var content = anchorFile.ReadString();
                            if (content == packageDirectory.Name)
                                continue;
                        }
                    }
                    bool success = true;
                    var temporaryDirectoryPath = anchoredDirectory.Parent.GetDirectory(anchoredDirectory.Name + ".old");
                    try
                    {
                        anchoredDirectory.MoveTo(temporaryDirectoryPath);

                        Anchor(packageDirectory, anchoredDirectory);
                    }
                    catch (Exception)
                    {
                        success = false;
                    }
                    if (success)
                    {
                        try
                        {
                            temporaryDirectoryPath.Delete();
                        }
                        catch (Exception)
                        {
                            success = false;
                        }
                    }
                    yield return new PackageAnchoredResult(this, package, success);
                }
                else
                {
                    Anchor(packageDirectory, anchoredDirectory);
                }
            }
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

        public IPackageInfo Publish(string packageFileName, Stream packageStream)
        {
            packageFileName = PackageNameUtility.NormalizeFileName(packageFileName);

            var wrapFile = BasePath.GetFile(packageFileName);
            if (wrapFile.Exists)
                return null;

            using (var file = wrapFile.OpenWrite())
                StreamExtensions.CopyTo(packageStream, file);

            var newPackageCacheDir = _rootCacheDirectory.GetDirectory(wrapFile.NameWithoutExtension);
            var newPackage = new CachedZipPackage(this, wrapFile, newPackageCacheDir, ExportBuilders.All);
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

        void Anchor(IDirectory packageDirectory, IDirectory anchoredDirectory)
        {
            var anchoredPath = anchoredDirectory.Path;
            if (_useSymLinks)
                packageDirectory.LinkTo(anchoredPath.FullPath);
            else
            {
                packageDirectory.CopyTo(anchoredDirectory);
                anchoredDirectory.GetFile(".anchored").WriteString(packageDirectory.Name);
            }
        }

        IPackageInfo CreatePackageInstance(IDirectory cacheDirectory, IFile wrapFile)
        {
            if (cacheDirectory.Exists)
                return new UncompressedPackage(this, wrapFile, cacheDirectory, ExportBuilders.All);
            return new CachedZipPackage(this, wrapFile, cacheDirectory, ExportBuilders.All);
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