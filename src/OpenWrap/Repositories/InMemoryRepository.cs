using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenWrap.Dependencies;

namespace OpenWrap.Repositories
{
    public class InMemoryRepository : IPackageRepository, ISupportPublishing, ISupportCleaning
    {
        IList<IPackageInfo> _packages = new List<IPackageInfo>();

        public InMemoryRepository(string name)
        {
            Name = name;
        }

        public bool CanPublish
        {
            get { return true; }
        }

        public void Refresh()
        {
        }

        public string Name
        {
            get; set;
        }

        public IEnumerable<IPackageInfo> Clean(IEnumerable<IPackageInfo> packagesToKepp)
        {
            var packagesToRemove = _packages.Where(x => !packagesToKepp.Contains(x)).ToList();
            _packages = packagesToKepp.ToList();
            return packagesToRemove;
        }

        public bool CanDelete
        {
            get { return true; }
        }

        public void Delete(IPackageInfo packageInfo)
        {
            _packages.Remove(packageInfo);
        }

        public ILookup<string, IPackageInfo> PackagesByName
        {
            get { return _packages.ToLookup(x => x.Name); }
        }

        public IList<IPackageInfo> Packages
        {
            get { return _packages; }
        }

        public IPackageInfo Find(PackageDependency dependency)
        {
            return PackagesByName.Find(dependency);
        }

        public IPackageInfo Publish(string packageFileName, Stream packageStream)
        {
            var fileWithoutExtension = packageFileName.Trim().ToLowerInvariant().EndsWith(".wrap")
                                               ? Path.GetFileNameWithoutExtension(packageFileName)
                                               : packageFileName;
            if (_packages.Any(x=>x.FullName == fileWithoutExtension))
                throw new InvalidOperationException("Package already exists in repository.");
                
            var package = new InMemoryPackage
            {
                    Name = PackageNameUtility.GetName(fileWithoutExtension),
                    Version = PackageNameUtility.GetVersion(fileWithoutExtension)
            };
            _packages.Add(package);
            return package;
        }
    }
}