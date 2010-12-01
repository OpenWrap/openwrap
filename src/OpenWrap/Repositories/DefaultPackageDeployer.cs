using System.Collections;
using System.Collections.ObjectModel;
using System.Reflection;
using OpenWrap.Commands;

namespace OpenWrap.Repositories
{
    public class DefaultPackageDeployer : IPackageDeployer
    {
        public void DeployDependency(IPackageInfo resolvedPackage,
                                     ISupportPublishing destinationRepository)
        {
            var source = resolvedPackage.Load();
            using (var packageStream = source.OpenStream())
                destinationRepository.Publish(resolvedPackage.FullName + ".wrap", packageStream);
            destinationRepository.RefreshPackages();
        }

        public void Initialize()
        {
        }
    }

//    public class PackageResolver : IPackageResolver
//    {
//        public DependencyResolutionResult TryResolveDependencies(PackageDescriptor packageDescriptor, IEnumerable<IPackageRepository> repositoriesToQuery)
//        {
//            return new DependencyResolutionResult(
//                ResolveAllDependencies(
//                    packageDescriptor.Dependencies,
//                    packageDescriptor.Overrides,
//                    repositoriesToQuery));
//        }



//        public void Initialize()
//        {
//        }

//        static PackageDependency ApplyAllWrapDependencyOverrides(IEnumerable<PackageNameOverride> dependencyOverrides, PackageDependency originalDependency)
//        {
//            return dependencyOverrides.Aggregate(originalDependency, (modifiedDependency, wrapOverride) => wrapOverride.Apply(modifiedDependency));
//        }

//        IEnumerable<ResolvedPackage> ResolveAllDependencies(IEnumerable<PackageDependency> dependencies, IEnumerable<PackageNameOverride> dependencyOverrides, IEnumerable<IPackageRepository> repositories)
//        {
//            var allResolvedDependencies = dependencies.SelectMany(x => Resolve(new ParentedDependency(x, null), dependencyOverrides, repositories, new List<PackageIdentifier>()));

//            foreach (var dependenciesForName in allResolvedDependencies
//.GroupBy(x => x.Dependency.Dependency.Name, StringComparer.OrdinalIgnoreCase))
//            {
//                // try to find packages present in all collections
//                var foundPackages = FindCommonPackage(dependenciesForName
//                    .Where(x => x.PackageFound)
//                    .Select(x => x.Packages)
//                    .NotNull());

//                var packagesByIdentifiers = from package in foundPackages
//                                            group package by package.Identifier into byId
//                                            orderby byId.Key.Version descending
//                                            select byId;

//                // no common package across all dependencies
//                var packageName = dependenciesForName.Key;
//                if (packagesByIdentifiers.Count() == 0)
//                {
//                    // try to apply local declarations
//                    var localOverride = dependencies.FirstOrDefault(x => x.Name.EqualsNoCase(packageName));
//                    if (localOverride != null)
//                    {
//                        var localPackage = repositories
//                            .SelectMany(x => x.FindAll(localOverride))
//                            .LatestVersions();
//                        var identifier = GetIdentifier(packageName, localPackage);
//                        yield return new ResolvedPackage(identifier, localPackage, dependenciesForName.Select(x => x.Dependency));
//                        continue;
//                    }

//                    // return the conflicting package
//                    foreach (var x in ReturnPackageErrors(dependenciesForName)) yield return x;
//                }
//                else
//                {
//                    var selectedPackages = foundPackages.LatestVersions();
//                    var identifier = GetIdentifier(packageName, selectedPackages);
//                    yield return new ResolvedPackage(identifier, selectedPackages, dependenciesForName.Select(x => x.Dependency).ToList());
//                }
//            }
//        }

//        PackageIdentifier GetIdentifier(string packageName, IEnumerable<IPackageInfo> selectedPackages)
//        {
//            if (packageName == null) throw new ArgumentNullException("packageName");
//            if (selectedPackages == null) throw new ArgumentNullException("selectedPackages");
//            return selectedPackages
//                    .Select(x => x.Identifier)
//                    .DefaultIfEmpty(new PackageIdentifier(packageName))
//                    .First();
//        }

//        static IEnumerable<ResolvedPackage> ReturnPackageErrors(IEnumerable<PackagesForDependency> dependenciesForName)
//        {
//            return (from x in dependenciesForName
//                    from package in x.Packages
//                    group new { package, x.Dependency } by package.Identifier
//                        into byId
//                        select new ResolvedPackage(byId.Key, byId.Select(x => x.package), byId.Select(x => x.Dependency))
//                   ).Concat(
//                    from x in dependenciesForName
//                    where x.Packages.Count() == 0
//                    group x by x.Dependency.Dependency.Name into byName
//                    select new ResolvedPackage(new PackageIdentifier(byName.Key), Enumerable.Empty<IPackageInfo>(), byName.Select(x => x.Dependency))
//                   );
//        }

//        IEnumerable<IPackageInfo> FindCommonPackage(IEnumerable<IEnumerable<IPackageInfo>> p)
//        {
//            var items = p.ToList();

//            return items.Count == 0
//                ? Enumerable.Empty<IPackageInfo>()
//                : (items.Count == 1
//                    ? items[0]
//                    : p.Skip(1).Aggregate(items[0], (first, second) => first.Intersect(second)));
//        }

//        IEnumerable<PackagesForDependency> Resolve(ParentedDependency dependency, IEnumerable<PackageNameOverride> dependencyOverrides, IEnumerable<IPackageRepository> repositories, List<PackageIdentifier> recursionPreventer)
//        {
//            var dependencyPostRewrite = ApplyAllWrapDependencyOverrides(dependencyOverrides, dependency.Dependency);
//            Func<PackageIdentifier, bool> packageNotAlreadyProcessed = x =>
//            {
//                if (recursionPreventer.Contains(x)) return false;
//                recursionPreventer.Add(x);
//                return true;
//            };
//            var packages = repositories.SelectMany(x => x.FindAll(dependencyPostRewrite)).ToList();
//            if (packages.Count == 0)
//                return new[] { PackagesForDependency.NotFound(dependency) };

//            return packages.Select(x => PackagesForDependency.Found(new ParentedDependency(dependencyPostRewrite, dependency.Parent), packages))
//                    .Concat(from package in packages.NotNull()
//                            where packageNotAlreadyProcessed(package.Identifier)
//                            from packageDependency in package.Dependencies
//                            from result in Resolve(new ParentedDependency(packageDependency, dependency), dependencyOverrides, repositories, recursionPreventer)
//                            select result);
//        }
//        class PackagesForDependency
//        {
//            private PackagesForDependency() { }
//            public static PackagesForDependency Found(ParentedDependency dependency, IEnumerable<IPackageInfo> packages)
//            {
//                return new PackagesForDependency
//                {
//                    Packages = packages,
//                    Dependency = dependency
//                };
//            }
//            public static PackagesForDependency NotFound(ParentedDependency dependency)
//            {
//                return new PackagesForDependency()
//                {
//                    Packages = Enumerable.Empty<IPackageInfo>(),
//                    Dependency = dependency
//                };
//            }
//            public bool PackageFound { get { return Packages.Count() > 0; } }
//            public IEnumerable<IPackageInfo> Packages { get; private set; }
//            public ParentedDependency Dependency { get; private set; }
//        }
//    }
}