using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OpenWrap.Commands;
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

        public DependencyResolutionResult TryResolveDependencies(PackageDescriptor packageDescriptor, IEnumerable<IPackageRepository> repositoriesToQuery)
        {
            var allDependencies = ResolveAllDependencies(packageDescriptor.Dependencies, packageDescriptor.Overrides, repositoriesToQuery);

            if (allDependencies.Any(x => x.Package == null))
                return SomeDependenciesNotFound(allDependencies);
            if (HasDependenciesConflict(allDependencies))
                allDependencies = OverrideDependenciesWithLocalDeclarations(allDependencies, packageDescriptor.Dependencies);
            if (HasDependenciesConflict(allDependencies))
                return ConflictingDependencies(allDependencies);
           


            return Successful(allDependencies);
        }


        public void UpdateDependency(ResolvedDependency dependency,
                                     ISupportPublishing destinationRepository)
        {
            var source = dependency.Package.Load();
            using (var packageStream = source.OpenStream())
                destinationRepository.Publish(dependency.Package.FullName + ".wrap", packageStream);
            destinationRepository.Refresh();
        }

        public void Initialize()
        {
        }

        PackageDependency ApplyAllWrapDependencyOverrides(IEnumerable<PackageNameOverride> dependencyOverrides, PackageDependency originalDependency)
        {
            return dependencyOverrides.Aggregate(originalDependency, (modifiedDependency, wrapOverride) => wrapOverride.Apply(modifiedDependency));
        }

        DependencyResolutionResult ConflictingDependencies(IEnumerable<ResolvedDependency> allDependencies)
        {
            return new DependencyResolutionResult { IsSuccess = false, Dependencies = allDependencies };
        }

        bool HasDependenciesConflict(IEnumerable<ResolvedDependency> resolutions)
        {
            return resolutions.ToLookup(x => x.Package.Name, StringComparer.OrdinalIgnoreCase).Any(x => x.Count() > 1);
        }

        IEnumerable<ResolvedDependency> OverrideDependenciesWithLocalDeclarations(IEnumerable<ResolvedDependency> dependencies, ICollection<PackageDependency> rootDependencies)
        {
            var overriddenDependencies = dependencies.ToList();

            foreach (var conflictingDependency in dependencies.ToLookup(x => x.Package.Name, StringComparer.OrdinalIgnoreCase).Where(x => x.Count() > 1))
            {
                var dependencyName = conflictingDependency.Key;
                var rootDependency = rootDependencies.FirstOrDefault(x => x.Name.EqualsNoCase(dependencyName));
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

        IEnumerable<ResolvedDependency> ResolveAllDependencies(IEnumerable<PackageDependency> dependencies, IEnumerable<PackageNameOverride> dependencyOverrides, IEnumerable<IPackageRepository> repositories)
        {
            var resolved = ResolveAllDependencies(null, dependencies, dependencyOverrides, repositories, new List<ResolvedDependency>());
            
            return resolved;
        }

        IEnumerable<ResolvedDependency> ResolveAllDependencies(IPackageInfo parent,
                                                               IEnumerable<PackageDependency> dependencies,
                                                               IEnumerable<PackageNameOverride> dependencyOverrides,
                                                               IEnumerable<IPackageRepository> repositories,
                                                               List<ResolvedDependency> resolvedDependencies)
        {
            IEnumerable<ResolvedDependency> packages = (from dependency in dependencies
                                                        let modifiedDependency = ApplyAllWrapDependencyOverrides(dependencyOverrides, dependency)
                                                        let package = repositories
                                                                .Where(x => x != null)
                                                                .Select(x => x.Find(modifiedDependency))
                                                                .NotNull()
                                                                .OrderByDescending(x=>x.Version)
                                                                .FirstOrDefault(x => x != null)
                                                        where package == null ||
                                                              resolvedDependencies.None(x => x.Package != null && x.Package.Name == package.Name && x.Package.Version == package.Version)
                                                        select new ResolvedDependency
                                                        {
                                                                Dependency = modifiedDependency,
                                                                Package = package,
                                                                ParentPackage = parent
                                                        }
                                                        ).ToList();

            resolvedDependencies.AddRange(packages);


            foreach (var package in packages.Where(x => x.Package != null))
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
            return new DependencyResolutionResult { IsSuccess = true, Dependencies = dependencies};
        }
    }
}