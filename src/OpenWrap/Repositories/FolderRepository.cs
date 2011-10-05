using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenFileSystem.IO;
using OpenRasta.Client;
using OpenWrap.Configuration;
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
    public class FolderRepository : ISupportCleaning, ISupportPublishing, ISupportAnchoring, IPackageRepository, ISupportLocking
    {
        readonly bool _anchoringEnabled;
        readonly IDirectory _rootCacheDirectory;
        readonly bool _useSymLinks;
        bool _canLock;
        IFile _lockFile;

        public FolderRepository(IDirectory packageBasePath, FolderRepositoryOptions options = FolderRepositoryOptions.Default)
        {
            _useSymLinks = (options & FolderRepositoryOptions.UseSymLinks) == FolderRepositoryOptions.UseSymLinks;
            _anchoringEnabled = (options & FolderRepositoryOptions.AnchoringEnabled) == FolderRepositoryOptions.AnchoringEnabled;
            _canLock = (options & FolderRepositoryOptions.SupportLocks) == FolderRepositoryOptions.SupportLocks;

            LockedPackages = Enumerable.Empty<IPackageInfo>().ToLookup(_ => string.Empty);

            if (packageBasePath == null) throw new ArgumentNullException("packageBasePath");

            BasePath = packageBasePath;


            _rootCacheDirectory = BasePath.GetOrCreateDirectory("_cache");
            RefreshPackages();
        }

        public IDirectory BasePath { get; set; }
        public string Name { get; set; }
        public string Type { get { return "Folder"; } }
        public string Token
        {
            get { return "[folder]" + BasePath.Path.FullPath; }
        }

        public TFeature Feature<TFeature>() where TFeature : class, IRepositoryFeature
        {
            if (typeof(TFeature) == typeof(ISupportAnchoring) && !_anchoringEnabled) return null;
            if (typeof(TFeature) == typeof(ISupportLocking) && !_canLock) return null;
            
            return this as TFeature;
        }

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
                        let package = CreatePackageInstance(packageCacheDirectory, wrapFile)
                        where package.IsValid
                        select new PackageLocation(
                                packageCacheDirectory,
                                package
                                )).ToList();
            RefreshLockFiles();
        }

        void RefreshLockFiles()
        {
            if (!_canLock) return;
            _lockFile = BasePath.Files("*.lock").Where(x => x.Name.StartsWith("packages.")).OrderBy(x => x.Name.Length).FirstOrDefault()
                        ?? BasePath.GetFile("packages.lock");
            if (_lockFile.Exists == false) return;

            var uri = new Uri(ConstantUris.URI_BASE, UriKind.Absolute).Combine(new Uri(_lockFile.Name, UriKind.Relative));
            var lockedPackageInfo = new DefaultConfigurationManager(BasePath).Load<LockedPackages>(uri)
                .Lock;
            var lockedPackages = lockedPackageInfo.Select(
                locked => Packages.Single(package => locked.Name.EqualsNoCase(package.Package.Name) &&
                                                     locked.Version == package.Package.Version).Package);

            LockedPackages = lockedPackages.ToLookup(x => string.Empty);
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
                    yield return new PackageAnchoredResult(package.Source, package, false);

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
            const string ANCHOR_MARKER_FILENAME = "_anchored";

            public override bool? Anchor(IDirectory packagesDirectory, IDirectory packageDirectory, string anchorName)
            {
                var anchoredDirectory = packagesDirectory.GetDirectory(anchorName);

                if (anchoredDirectory.Exists && anchoredDirectory.IsHardLink && !SafeDelete(anchoredDirectory))
                    return false;
                anchoredDirectory = packagesDirectory.GetDirectory(anchorName);
                if (anchoredDirectory.Exists)
                {
                    // convert old anchor file to new name
                    TryConvertOldAnchorMarker(anchoredDirectory);
                    var anchorFile = anchoredDirectory.GetFile(ANCHOR_MARKER_FILENAME);
                    if (anchorFile.Exists && anchorFile.ReadString() == packageDirectory.Name) return null;

                    if (!SafeDelete(anchoredDirectory)) return false;
                }
                packageDirectory.CopyTo(anchoredDirectory);
                anchoredDirectory.GetFile(ANCHOR_MARKER_FILENAME).WriteString(packageDirectory.Name);
                return true;
            }

            void TryConvertOldAnchorMarker(IDirectory anchoredDirectory)
            {
                var oldAnchor = anchoredDirectory.GetFile(".anchored");
                if (oldAnchor.Exists)
                {
                    var newAnchor = anchoredDirectory.GetFile(ANCHOR_MARKER_FILENAME);
                    if (newAnchor.Exists) newAnchor.Delete();
                    oldAnchor.MoveTo(newAnchor);
                }
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

        public void Lock(string scope, IEnumerable<IPackageInfo> currentPackages)
        {
            throw new NotImplementedException();
        }

        public ILookup<string, IPackageInfo> LockedPackages { get; private set; }
    }
    public class LockedPackages
    {
        public LockedPackages()
        {
            Lock = new List<LockedPackage>();

        }
        public IList<LockedPackage> Lock { get; set; }
    }

    public class LockedPackage
    {
        public LockedPackage()
        {
        }
        public LockedPackage(string name, Version version)
        {
            Name = name;
            Version = version;
        }

        public string Name { get; set; }
        public Version Version { get; set; }
    }
}