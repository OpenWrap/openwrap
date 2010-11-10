using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Commands;
using OpenWrap.Dependencies;
using OpenWrap.Exports;
using OpenWrap.Services;

namespace OpenWrap.Repositories
{
    public static class PackageResolverExtensions
    {
        public static IEnumerable<ICommandOutput> CopyPackagesToRepositories(this IPackageResolver resolver, DependencyResolutionResult dependencies, params IPackageRepository[] repositories)
        {
            return CopyPackagesToRepositories(resolver, dependencies, (IEnumerable<IPackageRepository>)repositories);
        }

        public static IEnumerable<ICommandOutput> CopyPackagesToRepositories(this IPackageResolver resolver,
                                                                             DependencyResolutionResult dependencies,
                                                                             IEnumerable<IPackageRepository> repositoriesToWriteTo)
        {
            if (resolver == null) throw new ArgumentNullException("resolver");
            if (dependencies == null) throw new ArgumentNullException("dependencies");
            if (repositoriesToWriteTo == null) throw new ArgumentNullException("repositoriesToWriteTo");

            if (!dependencies.IsSuccess)
            {
                yield return DependencyResolutionFailed(dependencies);
                yield break;
            }


            foreach (var dependency in dependencies.ResolvedPackages)
            {
                foreach (var repository in repositoriesToWriteTo.NotNull().OfType<ISupportPublishing>().ToList())
                {
                    var existingUpToDateVersion = repository.PackagesByName.Contains(dependency.Package.Name)
                                                          ? repository.PackagesByName[dependency.Package.Name]
                                                            .Where(x => x.Version >= dependency.Package.Version)
                                                            .OrderByDescending(x => x.Version)
                                                            .FirstOrDefault()
                                                          : null;
                    if (existingUpToDateVersion == null)
                    {
                        yield return new Result("'{0}' in '{1}': Updating to {2} from '{3}'", dependency.Package.Name, repository.Name, dependency.Package.Version, dependency.Package.Source.Name);
                        resolver.UpdateDependency(dependency, repository);
                    }
                    else
                    {
                        yield return new Result("'{0}' in '{1}': Up-to-date.", dependency.Package.Name, repository.Name);
                    }
                }
            }
        }

        // TODO: Expose at the pacakge resolver / repository level, such as a VerifyCache() or something along those lines...
        public static IEnumerable<ICommandOutput> VerifyPackageCache(this IPackageResolver packageResolver, IEnvironment environment, PackageDescriptor descriptor)
        {

            yield return new GenericMessage("Updating the package cache...");
            var repositories = (new[] { environment.ProjectRepository, environment.SystemRepository }).NotNull();

            packageResolver.GetExports<IExport>("bin", environment.ExecutionEnvironment, repositories).ToList();

            if (descriptor == null) yield break;

            var resolvedPackages = packageResolver.TryResolveDependencies(
                    descriptor,
                    repositories);

            foreach (var repo in repositories)
                repo.RefreshAnchors(resolvedPackages);
        }
        public static IPackageInfo LatestVersion(this IEnumerable<IPackageInfo> packages)
        {
            var packagesByVersion = packages
                    .NotNull()
                    .OrderByDescending(x => x.Version)
                    .ToList();
            return packagesByVersion
                    .Where(x=>x.Nuked == false)
                    .DefaultIfEmpty(packagesByVersion.FirstOrDefault())
                    .First();
        }
        static ICommandOutput DependencyResolutionFailed(DependencyResolutionResult result)
        {
            return new DependencyResolutionFailedResult(result);
        }
        public static IEnumerable<T> Concat<T>(this T value, IEnumerable<T> values)
        {
            return new[] { value }.Concat(values);
        }
    }
}