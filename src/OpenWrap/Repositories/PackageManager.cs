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
            dependencies = dependencies.ToList();
            var overriddenDependencies = dependencies.ToList();

            foreach (var conflictingDependencies in dependencies.ToLookup(x => x.Package.Name, StringComparer.OrdinalIgnoreCase).Where(x => x.Count() > 1))
            {
                var dependencyName = conflictingDependencies.Key;
                var rootDependency = rootDependencies.FirstOrDefault(x => x.Name.EqualsNoCase(dependencyName));
                var resolvedRootDependency = conflictingDependencies.FirstOrDefault(x => x.Dependency == rootDependency);
                if (resolvedRootDependency == null)
                    continue;
                foreach(var discarded in conflictingDependencies.Where(x=> x!= resolvedRootDependency))
                    overriddenDependencies.Remove(discarded);
            }
            return overriddenDependencies;
        }

        IEnumerable<ResolvedDependency> ResolveAllDependencies(IEnumerable<PackageDependency> dependencies, IEnumerable<PackageNameOverride> dependencyOverrides, IEnumerable<IPackageRepository> repositories)
        {
            return dependencies.SelectMany(x => ResolveDependency(null, x, dependencyOverrides, repositories, new List<IPackageInfo>()));
        }
        IEnumerable<ResolvedDependency> ResolveDependency(ResolvedDependency parent, PackageDependency dependencyToResolve, IEnumerable<PackageNameOverride> dependencyOverrides, IEnumerable<IPackageRepository> repositories, List<IPackageInfo> recursionPreventer)
        {
            dependencyToResolve = ApplyAllWrapDependencyOverrides(dependencyOverrides, dependencyToResolve);
            var package = GetLatestPackageVersion(repositories, dependencyToResolve);
            var dep = new ResolvedDependency
            {
                    Dependency = dependencyToResolve,
                    Package = package,
                    Parent = parent
            };
            if (package == null || recursionPreventer.Contains(package))
                return new[] { dep };

            recursionPreventer.Add(package);
            return dep.Concat(package.Dependencies.SelectMany(x => ResolveDependency(dep, x, dependencyOverrides, repositories, recursionPreventer))).ToList();
        }
        IEnumerable<ResolvedDependency> ResolveAllDependencies(IPackageInfo parent,
                                                               IEnumerable<PackageDependency> dependencies,
                                                               IEnumerable<PackageNameOverride> dependencyOverrides,
                                                               IEnumerable<IPackageRepository> repositories,
                                                               List<ResolvedDependency> resolvedDependencies)
        {
            IEnumerable<ResolvedDependency> packages = (from dependency in dependencies
                                                        let modifiedDependency = ApplyAllWrapDependencyOverrides(dependencyOverrides, dependency)
                                                        let package = GetLatestPackageVersion(repositories, modifiedDependency)
                                                        where package == null ||
                                                              resolvedDependencies.None(x => x.Package != null && x.Package.Name == package.Name && x.Package.Version == package.Version)
                                                        select new ResolvedDependency
                                                        {
                                                                Dependency = modifiedDependency,
                                                                Package = package
                                                                
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

        IPackageInfo GetLatestPackageVersion(IEnumerable<IPackageRepository> repositories, PackageDependency modifiedDependency)
        {
            var allPackages = repositories
                    .NotNull()
                    .Select(x => x.Find(modifiedDependency))
                    .NotNull()
                    .OrderByDescending(x=>x.Version);
            return allPackages
                    .FirstOrDefault(x => x != null);
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