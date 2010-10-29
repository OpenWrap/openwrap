using System.Linq;
using OpenWrap.Dependencies;

namespace OpenWrap.Repositories
{
    public static class PackagesExtensions
    {
        public static IPackageInfo Find(this ILookup<string, IPackageInfo> packages, PackageDependency dependency)
        {
            if (!packages.Contains(dependency.Name))
                return null;

            var allMatchingPackages = from package in packages[dependency.Name]
                                      where package.Version != null && dependency.IsFulfilledBy(package.Version)
                                      orderby package.Version descending
                                      select package;

            // only remove nuked versions if it's not an exact match
            var bestMatching = allMatchingPackages.FirstOrDefault();

            if (bestMatching != null)
            {
                if (dependency.IsExactlyFulfilledBy(bestMatching.Version))
                    return bestMatching;
            }

            // remove any nuked versions before returning the best match
            return (from package in allMatchingPackages
                    where !package.Nuked
                    select package).FirstOrDefault();
        }
    }
}
