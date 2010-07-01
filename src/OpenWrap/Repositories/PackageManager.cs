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
            var packageOverrides = GetOverrides(wrapDescriptor);
            var allDependencies = ResolveAllDependencies(wrapDescriptor.Dependencies, packageOverrides, repositoriesToQuery);

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

        DependencyResolutionResult ConflictingDependencies(IEnumerable<ResolvedDependency> allDependencies)
        {
            return new DependencyResolutionResult { IsSuccess = false, Dependencies = allDependencies };
        }

        Dictionary<string, string> GetOverrides(WrapDescriptor descriptor)
        {
            // TODO: Implement overrides
            return new Dictionary<string, string>();
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

        IEnumerable<ResolvedDependency> ResolveAllDependencies(IEnumerable<WrapDependency> dependencies, IDictionary<string, string> dependencyOverrides, IEnumerable<IPackageRepository> repositories)
        {
            return ResolveAllDependencies(null, dependencies, dependencyOverrides, repositories);
        }

        IEnumerable<ResolvedDependency> ResolveAllDependencies(IPackageInfo parent,
                                                               IEnumerable<WrapDependency> dependencies,
                                                               IDictionary<string, string> dependencyOverrides,
                                                               IEnumerable<IPackageRepository> repositories)
        {
            var packages = from dependency in dependencies
                           let package = repositories
                               .Where(x => x != null)
                               .Select(x => x.Find(dependency))
                               .FirstOrDefault(x => x != null)
                           select new ResolvedDependency { Dependency = dependency, Package = package, ParentPackage = parent };
            foreach (var package in packages.Where(p => p.Package != null))
                packages = packages.Concat(ResolveAllDependencies(package.Package, package.Package.Dependencies, dependencyOverrides, repositories));

            return packages.ToList();
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