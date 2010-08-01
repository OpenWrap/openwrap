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

                if (repositoriesForDependency.Count == 0)
                    repositoriesForDependency = repositoriesToWriteTo.ToList();


                foreach (var repository in repositoriesForDependency.Where(x => x != null && x.CanPublish))
                {
                    yield return new Result("Copying '{0}' to '{1}'", dependency.Package.FullName, repository.Name);
                    manager.UpdateDependency(dependency, repository);
                }
            }
        }
        public static IEnumerable<ICommandOutput> ExpandPackages(this IPackageManager packageManager, params IPackageRepository[] repositories)
        {
            yield return new GenericMessage("Expanding packages to cache...");
            
            packageManager.GetExports<IExport>("bin", WrapServices.GetService<IEnvironment>().ExecutionEnvironment, repositories.NotNull()).ToList();
        }
        public static IEnumerable<IPackageRepository> RepositoriesForRead(this IEnvironment environment)
        {
            return new[]
            {
                environment.CurrentDirectoryRepository,
                environment.ProjectRepository,
                environment.SystemRepository
            }.Concat(environment.RemoteRepositories);
        }

        public static IEnumerable<IPackageRepository> RepositoriesForWrite(this IEnvironment environment)
        {
            var reps = environment.RemoteRepositories.Concat(new[]
            {
                environment.CurrentDirectoryRepository,
                environment.SystemRepository
            }).ToList();
            if (environment.ProjectRepository != null)
                reps.Add(environment.ProjectRepository);
            return reps;
        }

        static ICommandOutput DependencyResolutionFailed(DependencyResolutionResult result)
        {
            return new Result("Dependency resolution failed.")
            {
                Success = false
            };
        }
    }
}