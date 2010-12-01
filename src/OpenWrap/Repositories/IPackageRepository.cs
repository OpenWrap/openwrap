using System.Collections.Generic;
using System.Linq;
using OpenWrap.Dependencies;

namespace OpenWrap.Repositories
{
    public interface IPackageRepository
    {
        ILookup<string, IPackageInfo> PackagesByName { get; }
        IPackageInfo Find(PackageDependency dependency);
        IEnumerable<IPackageInfo> FindAll(PackageDependency dependency);
        
        void RefreshPackages();
        string Name { get; }
    }
}