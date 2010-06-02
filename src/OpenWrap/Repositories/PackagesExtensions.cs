using System.Linq;
using OpenWrap.Dependencies;

namespace OpenWrap.Repositories
{
    public static class PackagesExtensions
    {
        public static IPackageInfo Find(this ILookup<string, IPackageInfo> packages, WrapDependency dependency)
        {
            if (!packages.Contains(dependency.Name))
                return null;
            return (from package in packages[dependency.Name]
                    where package.Version != null && dependency.IsFulfilledBy(package.Version)
                    orderby package.Version descending
                    select package).FirstOrDefault();
        }
    }
}