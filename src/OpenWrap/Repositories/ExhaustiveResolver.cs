using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Dependencies;

namespace OpenWrap.Repositories
{
    public class ExhaustiveResolver : IPackageResolver
    {
        const int MAX_RETRIES = 5000;
        public void Initialize()
        {
        }

        public DependencyResolutionResult TryResolveDependencies(
                PackageDescriptor packageDescriptor,
                IEnumerable<IPackageRepository> repositoriesToQuery)
        {
            var packageSelection = new PackageSelectionContext();

            for (int i = 0; i < MAX_RETRIES; i++)
            {
                var exclusionList = packageSelection.IncompatiblePackageVersions.Select(x => x.Key);

                var visitor = new DependencyVisitor(repositoriesToQuery, packageSelection, packageDescriptor.Dependencies, packageDescriptor.Overrides);
                var resolutionSucceeded = visitor
                        .VisitDependencies(packageDescriptor.Dependencies);

                if (resolutionSucceeded == false)
                {
                    var newExclusionList = packageSelection.IncompatiblePackageVersions.Select(x => x.Key);
                    if (newExclusionList.Except(exclusionList).Any())
                    {
                        packageSelection = new PackageSelectionContext(packageSelection.IncompatiblePackageVersions);

                        continue;
                    }
                }
                return Result(packageSelection, repositoriesToQuery, visitor.NotFound);
            }
            throw new InvalidOperationException(string.Format("OpenWrap tried {0} times to resolve the tree of dependencies and gave up.", MAX_RETRIES));
        }

        DependencyResolutionResult Result(PackageSelectionContext packageSelectionContext, IEnumerable<IPackageRepository> repositoriesToQuery, IEnumerable<IGrouping<PackageDependency, CallStack>> notFound)
        {
            var success = from compatible in packageSelectionContext.CompatiblePackageVersions
                          let id = compatible.Key
                          let packages = from repo in repositoriesToQuery
                                         from package in repo.FindAll(ToDependency(id))
                                         select package
                          select new ResolvedPackage(id, packages.ToList(), compatible.Value.Successful);
            var missing = from descriptor in notFound
                          select new ResolvedPackage(new PackageIdentifier(descriptor.Key.Name), Enumerable.Empty<IPackageInfo>(), descriptor.ToList());
            var conflicting = from incompat in packageSelectionContext.IncompatiblePackageVersions.GroupBy(x => x.Key, x => x.Value)
                              select new ResolvedPackage(incompat.Key, Enumerable.Empty<IPackageInfo>(), incompat.SelectMany(x => x.Failed.Concat(x.Successful)));

            return new DependencyResolutionResult(success, conflicting, missing);
        }

        PackageDependency ToDependency(PackageIdentifier pid)
        {
            return new PackageDependency
            {
                    Name = pid.Name,
                    VersionVertices = { new AbsolutelyEqualVersionVertex(pid.Version) }
            };
        }
    }
}