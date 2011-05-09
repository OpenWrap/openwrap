using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.PackageModel;

namespace OpenWrap.PackageManagement.DependencyResolvers
{
    public static class ResolutionResultExtensions
    {
        public static IEnumerable<IPackageInfo> NotInContentBranch(this DependencyResolutionResult source)
        {
            var allPackages = source.SuccessfulPackages.Select(x => x.Packages.First());
            return NotInContentBranch(allPackages, source.Descriptor.Dependencies);
        }
        public static IEnumerable<IPackageInfo> InContentBranch(this DependencyResolutionResult source)
        {
            var allPackages = source.SuccessfulPackages.Select(x => x.Packages.First());
            return InContentBranch(allPackages, source.Descriptor.Dependencies);
        }
        public static IEnumerable<IPackageInfo> NotInContentBranch(this IEnumerable<IPackageInfo> source, IEnumerable<PackageDependency> rootDependencies)
        {
            var packagesInContent = source.InContentBranch(rootDependencies).Select(x=>x.Name).ToList();

            return source.Where(package => packagesInContent.Contains(package.Name,StringComparer.OrdinalIgnoreCase) == false).ToList();
        }
        public static IEnumerable<IPackageInfo> InContentBranch(this IEnumerable<IPackageInfo> allPackages, IEnumerable<PackageDependency> rootDependencies)
        {
            var visitor = new PackageGraphVisitor(allPackages);
            var packagesInContent = allPackages.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
            var result = visitor.VisitFrom(rootDependencies, (from, dep, to) =>
            {
                // check if parent is in content branch already
                var parentInContentBranch = from != null && packagesInContent.ContainsKey(from.Name);
                if (parentInContentBranch == false && dep.ContentOnly == false)
                    packagesInContent.Remove(to.Name);
                return true;
            });
            return packagesInContent.Select(x=>x.Value);
        }
    }
}