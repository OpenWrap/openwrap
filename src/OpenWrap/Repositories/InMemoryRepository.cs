using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.InMemory;
using OpenWrap.PackageManagement;
using OpenWrap.PackageManagement.Packages;
using OpenWrap.PackageModel;

namespace OpenWrap.Repositories
{
    public class InMemoryRepository : IPackageRepository, 
        ISupportPublishing,
        ISupportCleaning,
        ISupportAuthentication,
        ISupportLocking
    {
        ICollection<IPackageInfo> _packages = new List<IPackageInfo>();

        public bool CanLock { get; set; }

        public InMemoryRepository(string name = null)
        {
            Name = name ?? string.Empty;
            CanPublish = true;
            Token = "[memory]" + Name;
        }

        public string Type { get { return "memory"; } }
        public bool CanAuthenticate { get; set; }

        public bool CanPublish { get; set; }

        public string Name { get; set; }

        public ICollection<IPackageInfo> Packages
        {
            get { return _packages; }
        }

        public ILookup<string, IPackageInfo> PackagesByName
        {
            get { return Packages.ToLookup(x => x.Name, StringComparer.OrdinalIgnoreCase); }
        }

        public string Token { get; set; }

        public TFeature Feature<TFeature>() where TFeature : class, IRepositoryFeature
        {
            if (typeof(TFeature) == typeof(ISupportPublishing) && !CanPublish)
                return null;
            if (typeof(TFeature) == typeof(ISupportAuthentication) && !CanAuthenticate)
                return null;
            if (typeof(TFeature) == typeof(ISupportLocking) && !CanLock)
                return null;
            return this as TFeature;
        }



        public void RefreshPackages()
        {
        }

        public IEnumerable<PackageCleanResult> Clean(IEnumerable<IPackageInfo> packagesToKeep)
        {
            var packagesToRemove = Packages.Where(x => !packagesToKeep.Contains(x)).ToList();
            _packages = packagesToKeep.ToList();
            return packagesToRemove.Select(x => new PackageCleanResult(x, true));
        }

        public IPackagePublisher Publisher()
        {
            return new PackagePublisher(Publish);
        }

        void Publish(IPackageRepository source, string packageFileName, Stream packageStream)
        {
            var fileWithoutExtension = packageFileName.Trim().ToLowerInvariant().EndsWith(".wrap")
                                           ? System.IO.Path.GetFileNameWithoutExtension(packageFileName)
                                           : packageFileName;
            if (Packages.Any(x => x.FullName.EqualsNoCase(fileWithoutExtension)))
                throw new InvalidOperationException("Package already exists in repository.");

            var inMemoryFile = new InMemoryFile("c:\\" + Guid.NewGuid());
            using (var stream = inMemoryFile.OpenWrite())
                IO.StreamExtensions.CopyTo(packageStream, stream);

            var tempFolder = new ZipFilePackage(inMemoryFile);

            var package = new InMemoryPackage(tempFolder) { Source = this };
            Packages.Add(package);
        }

        public IDisposable WithCredentials(NetworkCredential credentials)
        {
            return ActionOnDispose.None;
        }

        IDictionary<string, IEnumerable<IPackageInfo>> LockedPackages = new Dictionary<string,IEnumerable< IPackageInfo>>();

        public void Lock(string scope, IEnumerable<IPackageInfo> packages)
        {
            var namesToLock = packages.Select(x => x.Name).ToList();
            LockedPackages[scope] = LockedPackages.ContainsKey(scope)
                                        ? LockedPackages[scope]
                                            .Where(x=>!namesToLock.ContainsNoCase(x.Name))
                                            .Concat(packages)
                                            .ToList()
                                        : packages.ToList();
        }

        ILookup<string, IPackageInfo> ISupportLocking.LockedPackages
        {
            get
            {
                return (from kv in LockedPackages
                        from package in kv.Value
                        select new { kv.Key, package }).ToLookup(x=>x.Key, x=>x.package, StringComparer.OrdinalIgnoreCase);
            }
        }

        public void Unlock(string scope, IEnumerable<IPackageInfo> packages)
        {

            var namesToUnlock = packages.Select(x => x.Name).ToList();
            LockedPackages[scope] = LockedPackages.ContainsKey(scope)
                                        ? LockedPackages[scope]
                                            .Where(x => !namesToUnlock.ContainsNoCase(x.Name))
                                            .ToList()
                                        : Enumerable.Empty<IPackageInfo>();
        }
    }
}