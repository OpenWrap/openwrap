using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Dependencies;
using OpenWrap.Exports;

namespace OpenWrap.Repositories
{
    public class PackageManager : IPackageManager
    {
        public IEnumerable<T> GetExports<T>(string exportName, ExecutionEnvironment environment, IEnumerable<IPackageRepository> repositories) where T : IExport
        {
            var query = from repository in repositories
                        from packageName in repository.PackagesByName
                        let packageInfo = packageName.OrderByDescending(x => x.Version).FirstOrDefault()
                        select new { packageName, packageInfo };
            var latestPackages = from packageByName in query.ToLookup(x => x.packageName)
                                 select packageByName.FirstOrDefault().packageInfo.Load();

            return latestPackages.Select(x => x.GetExport(exportName, environment)).OfType<T>();
        }

        public DependencyResolutionResult TryResolveDependencies(WrapDescriptor wrapDescriptor, IEnumerable<IPackageRepository> repositoriesToQuery)
        {
            var allDependencies = ResolveAllDependencies(wrapDescriptor.Dependencies, wrapDescriptor.Overrides, repositoriesToQuery);

            if (allDependencies.Any(x => x.Package == null))
                return SomeDependenciesNotFound(allDependencies);
            if (HasDependenciesConflict(allDependencies))
                allDependencies = OverrideDependenciesWithLocalDeclarations(allDependencies, wrapDescriptor.Dependencies);
            if (HasDependenciesConflict(allDependencies))
                return ConflictingDependencies(allDependencies);

            return Successful(allDependencies);
        }

        public void UpdateDependency(ResolvedDependency dependency,
                                     IPackageRepository destinationRepository)
        {
            var source = dependency.Package.Load();
            using (var packageStream = source.OpenStream())
                destinationRepository.Publish(dependency.Package.FullName, packageStream);
        }

        public void Initialize()
        {
        }

        WrapDependency ApplyAllWrapDependencyOverrides(IEnumerable<WrapOverride> dependencyOverrides, WrapDependency originalDependency)
        {
            return dependencyOverrides.Aggregate(originalDependency, (modifiedDependency, wrapOverride) => wrapOverride.Apply(modifiedDependency));
        }

        DependencyResolutionResult ConflictingDependencies(IEnumerable<ResolvedDependency> allDependencies)
        {
            return new DependencyResolutionResult { IsSuccess = false, Dependencies = allDependencies };
        }

        bool HasDependenciesConflict(IEnumerable<ResolvedDependency> resolutions)
        {
            return resolutions.ToLookup(x => x.Package.Name).Any(x => x.Count() > 1);
        }

        IEnumerable<ResolvedDependency> OverrideDependenciesWithLocalDeclarations(IEnumerable<ResolvedDependency> dependencies, ICollection<WrapDependency> rootDependencies)
        {
            var overriddenDependencies = dependencies.ToList();

            foreach (var conflictingDependency in dependencies.ToLookup(x => x.Package.Name).Where(x => x.Count() > 1))
            {
                var dependencyName = conflictingDependency.Key;
                var rootDependency = rootDependencies.FirstOrDefault(x => x.Name == dependencyName);
                if (rootDependency == null)
                    continue;
                var rescuedDependency = conflictingDependency.OrderByDescending(x => x.Package.Version).FirstOrDefault(x => rootDependency.IsFulfilledBy(x.Package.Version));
                if (rescuedDependency == null)
                    continue;
                foreach (var toRemove in conflictingDependency.Where(x => x != rescuedDependency))
                    overriddenDependencies.Remove(toRemove);
            }
            return overriddenDependencies;
        }

        IEnumerable<ResolvedDependency> ResolveAllDependencies(IEnumerable<WrapDependency> dependencies, IEnumerable<WrapOverride> dependencyOverrides, IEnumerable<IPackageRepository> repositories)
        {
            return ResolveAllDependencies(null, dependencies, dependencyOverrides, repositories, new List<ResolvedDependency>());
        }

        IEnumerable<ResolvedDependency> ResolveAllDependencies(IPackageInfo parent,
                                                               IEnumerable<WrapDependency> dependencies,
                                                               IEnumerable<WrapOverride> dependencyOverrides,
                                                               IEnumerable<IPackageRepository> repositories,
                                                               List<ResolvedDependency> resolvedDependencies)
        {
            IEnumerable<ResolvedDependency> packages = (from dependency in dependencies
                            let modifiedDependency = ApplyAllWrapDependencyOverrides(dependencyOverrides, dependency)
                            let package = repositories
                                    .Where(x => x != null)
                                    .Select(x => x.Find(modifiedDependency))
                                    .FirstOrDefault(x => x != null)
                            where package == null ||
                                  resolvedDependencies.None(x => x.Package.Name == package.Name && x.Package.Version == package.Version)
                            select new ResolvedDependency { Dependency = modifiedDependency, Package = package, ParentPackage = parent })
                            .ToList();
            resolvedDependencies.AddRange(packages);


            foreach (var package in packages.Where(x=>x.Package != null))
                packages = packages.Concat(ResolveAllDependencies(
                    package.Package,
                    package.Package.Dependencies,
                    dependencyOverrides,
                    repositories,
                    resolvedDependencies));

            return packages;
        }

        DependencyResolutionResult SomeDependenciesNotFound(IEnumerable<ResolvedDependency> dependencies)
        {
            return new DependencyResolutionResult { IsSuccess = false, Dependencies = dependencies };
        }

        DependencyResolutionResult Successful(IEnumerable<ResolvedDependency> dependencies)
        {
            return new DependencyResolutionResult { IsSuccess = true, Dependencies = dependencies };
        }
    }
}