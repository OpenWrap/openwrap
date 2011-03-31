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
            var packagesInContent = GetPackageNamesInContentBranch(source, allPackages);

            return allPackages.Where(package => packagesInContent.Contains(package.Name) == false).ToList();
        }

        public static IEnumerable<IPackageInfo> InContentBranch(this DependencyResolutionResult source)
        {
            var allPackages = source.SuccessfulPackages.Select(x => x.Packages.First());
            var packagesInContent = GetPackageNamesInContentBranch(source, allPackages);

            return allPackages.Where(package => packagesInContent.Contains(package.Name)).ToList();
        }

        static List<string> GetPackageNamesInContentBranch(DependencyResolutionResult source, IEnumerable<IPackageInfo> allPackages)
        {
            var visitor = new PackageGraphVisitor(allPackages);
            var packagesInContent = allPackages.Select(x => x.Name).ToList();
            var result = visitor.VisitFromLeafs((from, dep, to) =>
            {
                // check if parent is in content branch already
                var parentInContentBranch = from != null && packagesInContent.Contains(from.Name, StringComparer.OrdinalIgnoreCase);
                if (parentInContentBranch == false && dep.ContentOnly == false)
                    packagesInContent.Remove(to.Name);
                return true;
            }, source.Descriptor.Dependencies);
            return packagesInContent;
        }
    }
}