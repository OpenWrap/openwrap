using System.Collections.Generic;
using System.Linq;
using OpenWrap.PackageModel;

namespace OpenWrap.Repositories
{
    public static class PackagesExtensions
    {

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