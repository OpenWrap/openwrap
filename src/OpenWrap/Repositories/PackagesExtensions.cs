using System.Collections.Generic;
using System.Linq;
using OpenWrap.PackageModel;

namespace OpenWrap.Repositories
{
    public static class PackagesExtensions
    {
        public static IPackageInfo Find(this IPackageRepository packages, PackageDependency dependency)
        {
            
            var allMatchingPackages = packages.FindAll(dependency);

            // only remove nuked versions if it's not an exact match
            var availVersion = allMatchingPackages.FirstOrDefault(x => x.Nuked == false);

            return availVersion ?? allMatchingPackages.FirstOrDefault();
        }

        public static IEnumerable<IPackageInfo> FindAll(this ILookup<string, IPackageInfo> packages, PackageDependency dependency)
        {
            if (!packages.Contains(dependency.Name))
                return Enumerable.Empty<IPackageInfo>();

            return (from package in packages[dependency.Name]
                    where package.Version != null && dependency.IsFulfilledBy(package.Version)
                    orderby package.Version descending
                    select package).ToList();
        }
    }

}