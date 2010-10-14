using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Commands;
using OpenWrap.Dependencies;
using OpenWrap.Exports;
using OpenWrap.Services;

namespace OpenWrap.Repositories
{
    public static class PackageManagerExtensions
    {
        public static IEnumerable<ICommandOutput> CopyPackagesToRepositories(this IPackageManager manager, DependencyResolutionResult dependencies, params IPackageRepository[] repositories)
        {
            return CopyPackagesToRepositories(manager, dependencies, (IEnumerable<IPackageRepository>)repositories);
        }

        public static IEnumerable<ICommandOutput> CopyPackagesToRepositories(this IPackageManager manager,
                                                                             DependencyResolutionResult dependencies,
                                                                             IEnumerable<IPackageRepository> repositoriesToWriteTo)
        {
            if (manager == null) throw new ArgumentNullException("manager");
            if (dependencies == null) throw new ArgumentNullException("dependencies");
            if (repositoriesToWriteTo == null) throw new ArgumentNullException("repositoriesToWriteTo");

            if (!dependencies.IsSuccess)
            {
                yield return DependencyResolutionFailed(dependencies);
                yield break;
            }


            foreach (var dependency in dependencies.Dependencies)
            {
                // fast forward to the source repository

                var repositoriesForDependency = repositoriesToWriteTo
                        .SkipWhile(x => x != dependency.Package.Source)
                        .Skip(1)
                        .ToList();

                foreach (var repository in repositoriesForDependency.NotNull().OfType<ISupportPublishing>().ToList())
                {
                    var existingUpToDateVersion = repository.PackagesByName.Contains(dependency.Package.Name)
                                                          ? repository.PackagesByName[dependency.Package.Name]
                                                            .Where(x => x.Version >= dependency.Package.Version)
                                                            .OrderByDescending(x => x.Version)
                                                            .FirstOrDefault()
                                                          : null;
                    if (existingUpToDateVersion == null)
                    {
                        yield return new Result("Copying '{0}' from '{1}' to '{2}'", dependency.Package.FullName, dependency.Package.Source.Name, repository.Name);
                        manager.UpdateDependency(dependency, repository);
                    }
                    else
                    {
                        yield return new Result("'{0}' up-to-date as '{1}' <= '{2}'.", repository.Name, dependency.Package.FullName, existingUpToDateVersion.FullName);
                    }
                }
            }
        }

        // TODO: Expose at the pacakge manager / repository level, such as a VerifyCache() or something along those lines...
        public static IEnumerable<ICommandOutput> VerifyPackageCache(this IPackageManager packageManager, IEnvironment environment, PackageDescriptor descriptor)
        {

            yield return new GenericMessage("Making sure the cache is up-to-date...");
            var repositories = (new[] { environment.ProjectRepository, environment.SystemRepository }).NotNull();

            var resolvedPackages = packageManager.TryResolveDependencies(
                    descriptor,
                    repositories);

            packageManager.GetExports<IExport>("bin", environment.ExecutionEnvironment, repositories).ToList();

            foreach (var repo in repositories)
                repo.RefreshAnchors(resolvedPackages);
        }

        public static IEnumerable<IPackageRepository> RepositoriesForRead(this IEnvironment environment)
        {
            return
                environment.RemoteRepositories
                .Concat(environment.CurrentDirectoryRepository,
                        environment.ProjectRepository,
                        environment.SystemRepository);
        }
        public static IEnumerable<IPackageRepository> RepositoriesForWrite(this IEnvironment environment)
        {
            return environment.RemoteRepositories
                    .Concat(environment.CurrentDirectoryRepository,
                            environment.SystemRepository,
                            environment.ProjectRepository)
                    .NotNull().ToList();
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