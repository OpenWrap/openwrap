using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenWrap.Build.Services;
using OpenWrap.Dependencies;
using OpenWrap.Repositories;

namespace OpenWrap.Repositories
{
    public interface IPackageManager : IService
    {
        DependencyResolutionResult TryResolveDependencies(WrapDescriptor wrapDescriptor, IPackageRepository projectRepository, IPackageRepository userRepository, IEnumerable<IPackageRepository> remoteRepositories);
    }

    public class ResolvedDependency
    {
        public WrapDependency Dependency { get; set; }
        public IPackageInfo Package { get; set; }
        public IPackageInfo ParentPackage { get; set; }
    }

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
                allDependencies = OverrideDependenciesWithLocalDeclarations(wrapDescriptor.Dependencies);
            if (HasDependenciesConflict(allDependencies))
                return ConflictingDependencies(allDependencies);

            return Successful(allDependencies);
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

        IEnumerable<ResolvedDependency> OverrideDependenciesWithLocalDeclarations(ICollection<WrapDependency> rootDependencies)
        {
            throw new NotImplementedException();
        }

        bool HasDependenciesConflict(IEnumerable<ResolvedDependency> resolutions)
        {
            return resolutions.Any(x => x.Package == null);
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

    public class DependencyResolutionResult
    {
        public IEnumerable<ResolvedDependency> Dependencies { get; set; }

        public bool IsSuccess { get; set; }
    }
}
