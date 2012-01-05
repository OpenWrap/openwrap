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
using OpenWrap.Repositories.FileSystem;
using StreamExtensions = OpenWrap.IO.StreamExtensions;


namespace OpenWrap.Repositories
{
    /// <summary>
    ///   Provides a repository that can read packages from a directory using the default structure.
    /// </summary>
    public class FolderRepository : ISupportCleaning, ISupportPublishing, ISupportAnchoring, IPackageRepository, ISupportLocking
    {
        readonly bool _anchoringEnabled;
        readonly PackageStorage _packageLoader;
        readonly bool _persistSource;
        readonly IDirectory _rootCacheDirectory;
        readonly bool _useSymLinks;
        bool _canLock;
        IFile _lockFile;
        Uri _lockFileUri;


        public FolderRepository(IDirectory packageBasePath, FolderRepositoryOptions options = FolderRepositoryOptions.Default)
        {
            if (packageBasePath == null) throw new ArgumentNullException("packageBasePath");
            _useSymLinks = (options & FolderRepositoryOptions.UseSymLinks) == FolderRepositoryOptions.UseSymLinks;
            _anchoringEnabled = (options & FolderRepositoryOptions.AnchoringEnabled) == FolderRepositoryOptions.AnchoringEnabled;
            _canLock = (options & FolderRepositoryOptions.SupportLocks) == FolderRepositoryOptions.SupportLocks;

            LockedPackages = Enumerable.Empty<IPackageInfo>().ToLookup(_ => string.Empty);

            _persistSource = (options & FolderRepositoryOptions.PersistPackageSources) == FolderRepositoryOptions.PersistPackageSources;

            BasePath = packageBasePath;


            _rootCacheDirectory = BasePath.GetOrCreateDirectory("_cache");

            _packageLoader = !_persistSource
                                 ? new PackageStorage(this, BasePath, _rootCacheDirectory)
                                 : new PackageStorageWithSource(this, BasePath, _rootCacheDirectory);
            if (_persistSource)
                packageBasePath.GetFile("packages").MustExist();
            RefreshPackages();
        }

        public IDirectory BasePath { get; set; }

        public string Name { get; set; }

        public string Type { get { return "Folder"; } }

        public ILookup<string, IPackageInfo> PackagesByName
        {
            get { return _packageLoader.Packages.Select(x => x.Package).ToLookup(x => x.Name, StringComparer.OrdinalIgnoreCase); }
        }

        public string Token
        {
            get { return "[folder]" + BasePath.Path.ToUri(); }
        }

        public TFeature Feature<TFeature>() where TFeature : class, IRepositoryFeature
        {
            if (typeof(TFeature) == typeof(ISupportAnchoring) && !_anchoringEnabled) return null;
            if (typeof(TFeature) == typeof(ISupportLocking) && !_canLock) return null;
            
            return this as TFeature;
        }

        public void RefreshPackages()
        {
            _packageLoader.Refresh();
            RefreshLockFiles();
        
        }

        void RefreshLockFiles()
        {
            if (!_canLock) return;
            _lockFile = BasePath.Files("*.lock").Where(x => x.Name.StartsWith("packages.")).OrderBy(x => x.Name.Length).FirstOrDefault()
                        ?? BasePath.GetFile("packages.lock");
            _lockFileUri = new Uri(ConstantUris.URI_BASE, UriKind.Absolute).Combine(new Uri(_lockFile.Name, UriKind.Relative));
            if (_lockFile.Exists == false) return;

            var lockedPackageInfo = new DefaultConfigurationManager(BasePath).Load<LockedPackages>(_lockFileUri)
                .Lock;
            var lockedPackages = lockedPackageInfo.Select(
                locked => _packageLoader.Packages.Single(package => locked.Name.EqualsNoCase(package.Package.Name) &&
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

                var success = anchorageStrategy.Anchor(BasePath, _packageLoader.Packages.First(x => x.Package == package).CacheDirectory, package.Name);
                if (success != null)
                    yield return new PackageAnchoredResult(this, package, (bool)success);
            }
        }


        void ChangeCompleted()
        {
            _packageLoader.PersistPackages();
            RefreshPackages();
        }

        public abstract class AnchorageStrategy
        {
            public abstract bool? Anchor(IDirectory packagesDirectory, IDirectory packageDirectory, string anchorName);
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
            public string Token { get; set; }
        }
        class DirectoryCopyAnchorageStrategy : AnchorageStrategy
        {
            const string ANCHOR_MARKER_FILENAME = "_anchored";

            public override bool? Anchor(IDirectory packagesDirectory, IDirectory packageDirectory, string anchorName)
            {
                var anchoredDirectory = packagesDirectory.GetDirectory(anchorName);

                if (anchoredDirectory.Exists && anchoredDirectory.IsHardLink && !anchoredDirectory.SafeDelete())
                    return false;
                anchoredDirectory = packagesDirectory.GetDirectory(anchorName);
                if (anchoredDirectory.Exists)
                {
                    // convert old anchor file to new name
                    TryConvertOldAnchorMarker(anchoredDirectory);
                    var anchorFile = anchoredDirectory.GetFile(ANCHOR_MARKER_FILENAME);
                    if (anchorFile.Exists && anchorFile.ReadString() == packageDirectory.Name) return null;

                    if (!anchoredDirectory.SafeDelete()) return false;
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
                    if (!anchoredDirectory.SafeDelete()) return false;
                }
                packageDirectory.LinkTo(anchoredDirectory.Path.FullPath);
                return true;
            }
        }       

        public class FolderPublisher : IPackagePublisherWithSource
        {
            readonly Func<IPackageRepository, string, Stream, IPackageInfo> _publish;
            readonly Action _publishCompleted;

            public FolderPublisher(Func<IPackageRepository, string, Stream, IPackageInfo> publish, Action publishCompleted)
            {
                _publish = publish;
                _publishCompleted = publishCompleted;
            }

            public void Dispose()
            {
                _publishCompleted();
            }

            public IPackageInfo Publish(string packageFileName, Stream packageStream)
            {
                return Publish(null, packageFileName, packageStream);
            }

            public IPackageInfo Publish(IPackageRepository source, string packageFileName, Stream packageStream)
            {
                return _publish(source, packageFileName, packageStream);
            }
        }

        class PackageStorage
        {
            protected IDirectory PackagesDirectory { get; private set; }
            readonly FolderRepository _parent;
            readonly IDirectory _rootCacheDirectory;
            IList<PackageLocation> _locationsReadonly;

            public PackageStorage(FolderRepository parent, IDirectory packagesDirectory, IDirectory rootCacheDirectory)
            {
                _parent = parent;
                PackagesDirectory = packagesDirectory;
                _rootCacheDirectory = rootCacheDirectory;
            }

            public IEnumerable<PackageLocation> Packages
            {
                get
                {
                    if (PackageLocations == null)
                        Refresh();
                    return _locationsReadonly;
                }
            }

            protected List<PackageLocation> PackageLocations { get; set; }

            public virtual void PersistPackages()
            {
                foreach (var package in Packages) package.Package.Load();
            }

            public virtual PackageLocation Publish(IPackageRepository source, string packageFileName, Stream packageStream)
            {
                var wrapFile = PackagesDirectory.GetFile(packageFileName);
                if (wrapFile.Exists)
                    throw new InvalidOperationException(string.Format("Cannot publish package to '{0}' as the file already exists.", wrapFile.Path));

                using (var file = wrapFile.OpenWrite())
                    IO.StreamExtensions.CopyTo(packageStream, file);

                var newPackageCacheDir = _rootCacheDirectory.GetDirectory(wrapFile.NameWithoutExtension);
                var newPackage = new CachedZipPackage(_parent, wrapFile, newPackageCacheDir);
                var packageLocation = new PackageLocation(newPackageCacheDir, newPackage);
                PackageLocations.Add(packageLocation);

                return packageLocation;
            }

            public void Refresh()
            {
                PackageLocations = LoadPackages().ToList();
                _locationsReadonly = PackageLocations.AsReadOnly();
            }
            public virtual IEnumerable<PackageLocation> LoadPackages()
            {
                return from wrapFile in PackagesDirectory.Files("*.wrap")
                       let packageFullName = wrapFile.NameWithoutExtension
                       let packageCacheDirectory = _rootCacheDirectory.GetDirectory(packageFullName)
                       let package = CreatePackageInstance(packageCacheDirectory, wrapFile)
                       where package.IsValid
                       select new PackageLocation(
                           packageCacheDirectory,
                           package
                           );
            }


            public virtual bool RemovePackage(PackageLocation packageInfo)
            {
                PackageLocations.Remove(packageInfo);
                if (packageInfo.CacheDirectory.TryDelete())
                {
                    PackagesDirectory.GetFile(packageInfo.Package.FullName + ".wrap").Delete();
                    return true;
                }
                return false;
            }

            IPackageInfo CreatePackageInstance(IDirectory cacheDirectory, IFile wrapFile)
            {
                if (cacheDirectory.Exists)
                    return new UncompressedPackage(_parent, wrapFile, cacheDirectory);
                return new CachedZipPackage(_parent, wrapFile, cacheDirectory);
            }
        }

        class PackageStorageWithSource : PackageStorage
        {
            PackageReferences _loadedPackages;

            public PackageStorageWithSource(FolderRepository parent, IDirectory packagesDirectory, IDirectory rootCacheDirectory) : base(parent, packagesDirectory, rootCacheDirectory)
            {

            }
            public override IEnumerable<PackageLocation> LoadPackages()
            {
                _loadedPackages = new DefaultConfigurationManager(PackagesDirectory).Load<PackageReferences>();
                
                var onDiskPackages = base.LoadPackages();
                return from packageRef in _loadedPackages.Packages
                       let package = onDiskPackages.FirstOrDefault(_ => PackageMatch(_, packageRef))
                                     ?? ReloadPackage(packageRef)
                       where package != null
                       select package;
            }

            bool PackageMatch(PackageLocation location, PackageReference packageRef)
            {
                return location.Package.Name.EqualsNoCase(packageRef.Name) &&
                       location.Package.Version.Equals(packageRef.Version);
            }
            public override PackageLocation Publish(IPackageRepository source, string packageFileName, Stream packageStream)
            {
                var location = base.Publish(source, packageFileName, packageStream);
                var packageRef = new PackageReference()
                {
                    Name = location.Package.Name,
                    Version = location.Package.Version
                };
                if (source != null)
                    packageRef.Source = source.Token;
                _loadedPackages.Packages.Add(packageRef);
                return location;
            }
            public override bool RemovePackage(PackageLocation packageInfo)
            {
                var packageRef = _loadedPackages.Packages.FirstOrDefault(_ => PackageMatch(packageInfo, _));
                if (packageRef == null) return false;
                _loadedPackages.Packages.Remove(packageRef);
                base.RemovePackage(packageInfo);
                // ignore failure to remove packages as we now have a list
                return true;
            }
            public override void PersistPackages()
            {
                base.PersistPackages();
                new DefaultConfigurationManager(PackagesDirectory).Save(_loadedPackages);
            }
            PackageLocation ReloadPackage(PackageReference packageRef)
            {
                // TODO: Implement auto-reload of package
                throw new InvalidOperationException();
            }
        }
        [Path("packages")]
        public class PackageReferences
        {
            public PackageReferences()
            {
                Packages = new List<PackageReference>();
            }
            [Key("package")]
            public ICollection<PackageReference> Packages { get; set; }
            public static readonly PackageReferences Default = new PackageReferences();
        }
        public class PackageReference
        {
            public string Name { get; set; }
            public string Source { get; set; }
            public Version Version { get; set; }
        }
    
        public IEnumerable<PackageCleanResult> Clean(IEnumerable<IPackageInfo> packagesToKeep)
        {
            packagesToKeep = packagesToKeep.ToList();
            var packagesToRemove = _packageLoader.Packages.Where(x => !packagesToKeep.Contains(x.Package)).ToList();
            foreach (var packageInfo in packagesToRemove)
            {
                yield return new PackageCleanResult(packageInfo.Package, _packageLoader.RemovePackage(packageInfo));
            }
            ChangeCompleted();
        }

        public IPackageInfo Publish(IPackageRepository repository, string packageFileName, Stream packageStream)
        {
            packageFileName = PackageNameUtility.NormalizeFileName(packageFileName);
            var newPackage = _packageLoader.Publish(repository, packageFileName, packageStream);

            return newPackage.Package;
        }


        public IPackagePublisher Publisher()
        {
            return new FolderPublisher(Publish, ChangeCompleted);
        }


        public void Lock(string scope, IEnumerable<IPackageInfo> packages)
        {
            if (scope != string.Empty) throw new NotImplementedException("Can only lock in default scope.");
            var packageNames = packages.Select(x => x.Name).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var locked = LockedPackages[scope].Where(_=>packageNames.ContainsNoCase(_.Name) == false).Concat(packages)
                .Select(x=>new LockedPackage(x.Name, x.Version)).ToList();
            
            new DefaultConfigurationManager(BasePath).Save(new LockedPackages { Lock = locked }, _lockFileUri);
            RefreshLockFiles();
        }

        public ILookup<string, IPackageInfo> LockedPackages { get; private set; }

        public void Unlock(string scope, IEnumerable<IPackageInfo> packages)
        {
            if (scope != string.Empty) throw new NotImplementedException("Can only lock in default scope.");

            var packageNames = packages.Select(x => x.Name).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var locked = LockedPackages[scope].Where(_ => packageNames.ContainsNoCase(_.Name) == false)
                .Select(x => new LockedPackage(x.Name, x.Version)).ToList();

            new DefaultConfigurationManager(BasePath).Save(new LockedPackages { Lock = locked }, _lockFileUri);
            RefreshLockFiles();
        }
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
