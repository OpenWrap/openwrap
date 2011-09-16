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
        ISupportAuthentication
    {
        ICollection<IPackageInfo> _packages = new List<IPackageInfo>();

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
            return this as TFeature;
        }


        public IEnumerable<IPackageInfo> FindAll(PackageDependency dependency)
        {
            return PackagesByName.FindAll(dependency);
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

        IPackageInfo Publish(string packageFileName, Stream packageStream)
        {
            var fileWithoutExtension = packageFileName.Trim().ToLowerInvariant().EndsWith(".wrap")
                                           ? System.IO.Path.GetFileNameWithoutExtension(packageFileName)
                                           : packageFileName;
            if (Packages.Any(x => x.FullName.EqualsNoCase(fileWithoutExtension)))
                throw new InvalidOperationException("Package already exists in repository.");

            var inMemoryFile = new InMemoryFile("c:\\" + Guid.NewGuid());
            using (var stream = FileExtensions.OpenWrite(inMemoryFile))
                IO.StreamExtensions.CopyTo(packageStream, stream);

            var tempFolder = new ZipPackage(inMemoryFile);

            var package = new InMemoryPackage
            {
                Name = PackageNameUtility.GetName(fileWithoutExtension),
                Version = PackageNameUtility.GetVersion(fileWithoutExtension),
                Source = this,
                Dependencies = tempFolder.Dependencies.ToList(),
                Anchored = tempFolder.Anchored
            };
            Packages.Add(package);
            return package;
        }

        public IDisposable WithCredentials(NetworkCredential credentials)
        {
            return ActionOnDispose.None;
        }
    }
}