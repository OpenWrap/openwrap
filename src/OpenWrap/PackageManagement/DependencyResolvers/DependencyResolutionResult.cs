using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Collections;
using OpenWrap.PackageModel;

namespace OpenWrap.PackageManagement.DependencyResolvers
{
    public class DependencyResolutionResult
    {
        public IPackageDescriptor Descriptor { get; private set; }

        public DependencyResolutionResult(IPackageDescriptor descriptor, IEnumerable<ResolvedPackage> successfulPackages, IEnumerable<ResolvedPackage> conflictingPackages, IEnumerable<ResolvedPackage> missingPackages)
        {
            Descriptor = descriptor;
            SuccessfulPackages = successfulPackages.ToList().AsReadOnly();
            DiscardedPackages = conflictingPackages.ToList().AsReadOnly();
            MissingPackages = missingPackages.ToList().AsReadOnly();
            //IsSuccess = !(MissingPackages.Any() || DiscardedPackages.Any());
            IsSuccess = !(MissingPackages.Any(x => SuccessfulPackages.None(s => s.Identifier.Name.EqualsNoCase(x.Identifier.Name)))
                          || DiscardedPackages.Any(x => SuccessfulPackages.None(s => s.Identifier.Name.EqualsNoCase(x.Identifier.Name))));
        }

        public IEnumerable<ResolvedPackage> DiscardedPackages { get; private set; }

        public bool IsSuccess { get; private set; }
        public IEnumerable<ResolvedPackage> MissingPackages { get; private set; }
        public IEnumerable<ResolvedPackage> SuccessfulPackages { get; private set; }

    }
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
            var packagesInContent = source.InContentBranch(rootDependencies).Select(x=>x.Name);

            return source.Where(package => packagesInContent.Contains(package.Name,StringComparer.OrdinalIgnoreCase) == false).ToList();
        }
        public static IEnumerable<IPackageInfo> InContentBranch(this IEnumerable<IPackageInfo> allPackages, IEnumerable<PackageDependency> rootDependencies)
        {
            var visitor = new PackageGraphVisitor(allPackages);
            var packagesInContent = allPackages.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
            var result = visitor.VisitFromLeafs((from, dep, to) =>
            {
                // check if parent is in content branch already
                var parentInContentBranch = from != null && packagesInContent.ContainsKey(from.Name);
                if (parentInContentBranch == false && dep.ContentOnly == false)
                    packagesInContent.Remove(to.Name);
                return true;
            }, rootDependencies);
            return packagesInContent.Select(x=>x.Value);
        }
    }
}