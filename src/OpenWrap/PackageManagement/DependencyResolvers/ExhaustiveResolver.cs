using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;

namespace OpenWrap.PackageManagement.DependencyResolvers
{
    public class StrategyResolver : IPackageResolver
    {
        const int MAX_RETRIES = 5000;
        public void Initialize()
        {
        }

        public DependencyResolutionResult TryResolveDependencies(IPackageDescriptor packageDescriptor, IEnumerable<IPackageRepository> repositoriesToQuery)
        {
            Debug.WriteLine("Resolving descriptor " + packageDescriptor.Dependencies.Select(x => x.ToString()).JoinString(", "));

            List<IPackageInfo> excluded = new List<IPackageInfo>();

            var allPackages = repositoriesToQuery.Packages().SelectMany(_=>_);

            var declaredDependencies = packageDescriptor.Dependencies.Select(PackageMatchesDependency).ToList();
            for (int i = 0; i < MAX_RETRIES; i++)
            {
                var visitor = new LoggingPackageResolver(allPackages, fail: excluded);

                if (visitor.Visit(declaredDependencies))
                {
                    return new DependencyResolutionResult(packageDescriptor, 
                        ToResolveResult(visitor.Success),
                        ToResolveResult(visitor.Fail),
                        ToResolveResult(visitor.NotFound));
                }
                var newExclusions = visitor.IncompatiblePackages.Except(excluded).ToList();
                if (newExclusions.Any() == false) break;

                excluded.AddRange(visitor.IncompatiblePackages);
            }
            throw new InvalidOperationException(string.Format("OpenWrap tried {0} times to resolve the tree of dependencies and gave up.", MAX_RETRIES));
        }

        IEnumerable<ResolvedPackage> ToResolveResult(List<CallStack> success)
        {
            return Enumerable.Empty<ResolvedPackage>();
        }

        Func<IPackageInfo, bool> GetDependency(Dictionary<PackageDependency, Func<IPackageInfo, bool>> dependencyMap, PackageDependency packageDependency)
        {
            return dependencyMap.GetOrCreate(packageDependency, () => PackageMatchesDependency(packageDependency));
        }


        Func<IPackageInfo, bool> PackageMatchesDependency(PackageDependency packageDependency)
        {
            return package => packageDependency.IsFulfilledBy(package.SemanticVersion ?? package.Version.ToSemVer());
        }
    }
    public class ExhaustiveResolver : IPackageResolver
    {
        const int MAX_RETRIES = 5000;

        public DependencyResolutionResult TryResolveDependencies(IPackageDescriptor packageDescriptor, IEnumerable<IPackageRepository> repositoriesToQuery)
        {
            var packageSelection = new PackageSelectionContext();

                Debug.WriteLine("Resolving descriptor " + packageDescriptor.Dependencies.Select(x => x.ToString()).JoinString(", "));
            for (int i = 0; i < MAX_RETRIES; i++)
            {
                var exclusionList = packageSelection.IncompatiblePackageVersions.Select(x => x.Key);

                var visitor = new DependencyVisitor(repositoriesToQuery.Packages(), packageSelection, packageDescriptor.Dependencies, packageDescriptor.Overrides);
                var resolutionSucceeded = visitor.VisitDependencies(packageDescriptor.Dependencies);

                if (resolutionSucceeded == false)
                {
                    var newExclusionList = packageSelection.IncompatiblePackageVersions.Select(x => x.Key);
                    if (newExclusionList.Except(exclusionList).Any())
                    {
                        packageSelection = new PackageSelectionContext(packageSelection.IncompatiblePackageVersions);

                        continue;
                    }
                }
                return Result(packageDescriptor, packageSelection, repositoriesToQuery, visitor.NotFound);
            }
            throw new InvalidOperationException(string.Format("OpenWrap tried {0} times to resolve the tree of dependencies and gave up.", MAX_RETRIES));
        }

        public void Initialize()
        {
        }

        DependencyResolutionResult Result(IPackageDescriptor descriptor, 
                                          PackageSelectionContext packageSelectionContext,
                                          IEnumerable<IPackageRepository> repositoriesToQuery,
                                          IEnumerable<IGrouping<PackageDependency, CallStack>> notFound)
        {
            var success = (from compatible in packageSelectionContext.CompatiblePackageVersions
                           let id = compatible.Key
                           let packages = from repo in repositoriesToQuery
                                          from package in repo.PackagesByName.FindAll(ToDependency(id))
                                          select package
                           select new ResolvedPackage(id, packages.ToList(), compatible.Value.Successful))
                    .ToList();
            var missing = from callStacks in notFound
                          select new ResolvedPackage(new PackageIdentifier(callStacks.Key.Name), Enumerable.Empty<IPackageInfo>(), callStacks.ToList());
            var conflicting = from incompat in packageSelectionContext.IncompatiblePackageVersions.GroupBy(x => x.Key, x => x.Value)
                              select new ResolvedPackage(incompat.Key, Enumerable.Empty<IPackageInfo>(), incompat.SelectMany(x => x.Failed.Concat(x.Successful)));

            return new DependencyResolutionResult(descriptor, success, conflicting, missing);
        }

        PackageDependency ToDependency(PackageIdentifier pid)
        {
            return new PackageDependencyBuilder(pid.Name).VersionVertex(new AbsolutelyEqualVersionVertex(pid.Version));
        }
    }
}