using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OpenWrap.Commands;
using OpenWrap.Dependencies;
using OpenWrap.Exports;

namespace OpenWrap.Repositories
{
    public class PackageResolver : IPackageResolver
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
                return ConflictingDependencies(allDependencies);

            return Successful(allDependencies);
        }

        public void UpdateDependency(ResolvedPackage resolvedPackage,
                                     ISupportPublishing destinationRepository)
        {
            var source = resolvedPackage.Package.Load();
            using (var packageStream = source.OpenStream())
                destinationRepository.Publish(resolvedPackage.Package.FullName + ".wrap", packageStream);
            destinationRepository.Refresh();
        }

        public void Initialize()
        {
        }

        PackageDependency ApplyAllWrapDependencyOverrides(IEnumerable<PackageNameOverride> dependencyOverrides, PackageDependency originalDependency)
        {
            return dependencyOverrides.Aggregate(originalDependency, (modifiedDependency, wrapOverride) => wrapOverride.Apply(modifiedDependency));
        }

        DependencyResolutionResult ConflictingDependencies(IEnumerable<ResolvedPackage> allDependencies)
        {
            return new DependencyResolutionResult { IsSuccess = false, ResolvedPackages = allDependencies };
        }

        bool HasDependenciesConflict(IEnumerable<ResolvedPackage> resolutions)
        {
            return (from resolutionsByName in resolutions.ToLookup(x => x.Package.Name, StringComparer.OrdinalIgnoreCase)
                    where resolutionsByName.Count() > 1
                    let packages = resolutionsByName.Select(x => x.Package).Distinct()
                    where packages.Count() > 1
                    select resolutionsByName).Any();
        }

        IEnumerable<ResolvedPackage> ResolveAllDependencies(IEnumerable<PackageDependency> dependencies, IEnumerable<PackageNameOverride> dependencyOverrides, IEnumerable<IPackageRepository> repositories)
        {
            var allResolvedDependencies = dependencies.SelectMany(x => Resolve(new ParentedDependency(x, null), dependencyOverrides, repositories, new List<IPackageInfo>()));

            foreach (var dependenciesForName in allResolvedDependencies
.GroupBy(x => x.dependency.Dependency.Name, StringComparer.OrdinalIgnoreCase))
            {
                // try to find an element present in all collections
                var foundPackages = FindCommonPackage(dependenciesForName
                    .Where(x=>x.PackageFound)
                    .Select(x => x.packages)
                    .NotNull());

                if (foundPackages.Count() == 0)
                {
                    // try to apply local declarations
                    var localOverride = dependencies.FirstOrDefault(x => x.Name.EqualsNoCase(dependenciesForName.Key));
                    if (localOverride != null)
                    {
                        var localPackage = repositories
                            .SelectMany(x => x.FindAll(localOverride))
                            .LatestVersion();
                        yield return new ResolvedPackage(dependenciesForName.Key, localPackage, dependenciesForName.Select(x => x.dependency));
                        continue;
                    }

                    // return the conflicting package
                    foreach (var x in ReturnConflictingPacakges(dependenciesForName)) yield return x;
                }
                else
                {
                    yield return new ResolvedPackage
                    (
                        dependenciesForName.Key,
                        foundPackages.LatestVersion(),
                        dependenciesForName.Select(x => x.dependency).ToList()
                    );
                }
            }
        }

        IEnumerable<ResolvedPackage> ReturnConflictingPacakges(IGrouping<string, R> dependenciesForName)
        {
            return from perPackage in
                           from resolved in dependenciesForName
                           from package in resolved.packages
                           group new { package, resolved } by package
                   select new ResolvedPackage(dependenciesForName.Key, perPackage.Key, perPackage.Select(x => x.resolved.dependency));
        }

        IEnumerable<IPackageInfo> FindCommonPackage(IEnumerable<IEnumerable<IPackageInfo>> p)
        {
            var items = p.ToList();

            return items.Count == 0
                ? Enumerable.Empty<IPackageInfo>()
                : (items.Count == 1
                    ? items[0]
                    : p.Skip(1).Aggregate(items[0], (first, second) => first.Intersect(second)));
        }

        IEnumerable<R> Resolve(ParentedDependency dependency, IEnumerable<PackageNameOverride> dependencyOverrides, IEnumerable<IPackageRepository> repositories, List<IPackageInfo> recursionPreventer)
        {
            var dependencyPostRewrite = ApplyAllWrapDependencyOverrides(dependencyOverrides, dependency.Dependency);
            Func<IPackageInfo, bool> packageNotAlreadyProcessed = x =>
            {
                if (recursionPreventer.Contains(x)) return false;
                recursionPreventer.Add(x);
                return true;
            };
            var packages = repositories.SelectMany(x => x.FindAll(dependencyPostRewrite)).ToList();
            if (packages.Count == 0)
                return new[] { R.NotFound(dependency) };

            return packages.Select(x => R.Found(new ParentedDependency(dependencyPostRewrite, dependency.Parent), packages))
                    .Concat(from package in packages.NotNull()
                            where packageNotAlreadyProcessed(package)
                            from packageDependency in package.Dependencies
                            from result in Resolve(new ParentedDependency(packageDependency, dependency), dependencyOverrides, repositories, recursionPreventer)
                            select result);
        }
        class R
        {
            private R() { }
            public static R Found(ParentedDependency dependency, IEnumerable<IPackageInfo> packages)
            {
                return new R
                {
                    packages = packages,
                    dependency = dependency
                };
            }
            public static R NotFound(ParentedDependency dependency)
            {
                return new R()
                {
                    packages = Enumerable.Empty<IPackageInfo>(),
                    dependency = dependency
                };
            }
            public bool PackageFound { get { return packages.Count() > 0; } }
            public IEnumerable<IPackageInfo> packages;
            public ParentedDependency dependency;
        }

        DependencyResolutionResult SomeDependenciesNotFound(IEnumerable<ResolvedPackage> dependencies)
        {
            return new DependencyResolutionResult { IsSuccess = false, ResolvedPackages = dependencies };
        }

        DependencyResolutionResult Successful(IEnumerable<ResolvedPackage> dependencies)
        {
            return new DependencyResolutionResult { IsSuccess = true, ResolvedPackages = dependencies };
        }
    }
}