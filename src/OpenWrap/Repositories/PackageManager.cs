using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Dependencies;

namespace OpenWrap.Repositories
{
    public class PackageManager : IPackageManager
    {
        public void Initialize()
        {

        }

        public DependencyResolutionResult TryResolveDependencies(WrapDescriptor wrapDescriptor, IPackageRepository projectRepository, IPackageRepository userRepository, IEnumerable<IPackageRepository> remoteRepositories)
        {
            var repositories = new[] { projectRepository, userRepository }.Concat(remoteRepositories);

            var packageOverrides = GetOverrides(wrapDescriptor);
            var allDependencies = ResolveAllDependencies(wrapDescriptor.Dependencies, packageOverrides, repositories);

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
            var sourceRepository = dependency.Package.Source;
            var source = dependency.Package.Load();
            using(var packageStream = source.OpenStream())
            {
                destinationRepository.Publish(dependency.Package.Name + "-" + dependency.Package.Version, packageStream);
            }
        }

        DependencyResolutionResult Successful(IEnumerable<ResolvedDependency> dependencies)
        {
            return new DependencyResolutionResult { IsSuccess = true, Dependencies = dependencies };
        }

        DependencyResolutionResult ConflictingDependencies(IEnumerable<ResolvedDependency> allDependencies)
        {
            return new DependencyResolutionResult { IsSuccess = false, Dependencies = allDependencies };
        }

        DependencyResolutionResult SomeDependenciesNotFound(IEnumerable<ResolvedDependency> dependencies)
        {
            return new DependencyResolutionResult { IsSuccess = false, Dependencies = dependencies };
        }

        Dictionary<string, string> GetOverrides(WrapDescriptor descriptor)
        {
            return new Dictionary<string, string>();
        }

        IEnumerable<ResolvedDependency> OverrideDependenciesWithLocalDeclarations(IEnumerable<ResolvedDependency> dependencies, ICollection<WrapDependency> rootDependencies)
        {
            var overriddenDependencies = dependencies.ToList();

            foreach(var conflictingDependency in dependencies.ToLookup(x=>x.Package.Name).Where(x=>x.Count() > 1))
            {
                var dependencyName = conflictingDependency.Key;
                var rootDependency = rootDependencies.FirstOrDefault(x => x.Name == dependencyName);
                if (rootDependency == null)
                    continue;
                var rescuedDependency = conflictingDependency.OrderByDescending(x => x.Package.Version).FirstOrDefault(x => rootDependency.IsFulfilledBy(x.Package.Version));
                if (rescuedDependency == null)
                    continue;
                foreach (var toRemove in conflictingDependency.Where(x=>x != rescuedDependency))
                    overriddenDependencies.Remove(toRemove);

            }
            return overriddenDependencies;
        }

        bool HasDependenciesConflict(IEnumerable<ResolvedDependency> resolutions)
        {
            return resolutions.ToLookup(x => x.Package.Name).Any(x => x.Count() > 1);
        }

        IEnumerable<ResolvedDependency> ResolveAllDependencies(IEnumerable<WrapDependency> dependencies, IDictionary<string,string> dependencyOverrides, IEnumerable<IPackageRepository> repositories)
        {
            return ResolveAllDependencies(null, dependencies, dependencyOverrides, repositories);
        }

        IEnumerable<ResolvedDependency> ResolveAllDependencies(IPackageInfo parent, IEnumerable<WrapDependency> dependencies, IDictionary<string,string> dependencyOverrides, IEnumerable<IPackageRepository> repositories)
        {
            var packages = from dependency in dependencies
                           let package = repositories.Select(x => x.Find(dependency)).FirstOrDefault(x=>x != null)
                           select new ResolvedDependency { Dependency = dependency, Package = package, ParentPackage = parent };
            foreach (var package in packages.Where(p=>p.Package != null))
                packages = packages.Concat(ResolveAllDependencies(package.Package, package.Package.Dependencies, dependencyOverrides, repositories));

            return packages.ToList();
        }
    }
}