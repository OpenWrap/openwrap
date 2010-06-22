using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Commands;
using OpenWrap.Commands.Core;

namespace OpenWrap.Repositories
{
    public static class PackageManagerExtensions
    {
        static ICommandResult DependencyResolutionFailed(DependencyResolutionResult result)
        {
            return new Result("Dependency resolution failed.")
            {
                Success = false
            };
        }

        public static IEnumerable<ICommandResult> CopyResolvedDependenciesToRepositories(this IPackageManager manager, DependencyResolutionResult dependencies, params IPackageRepository[] repositories)
        {
            return CopyResolvedDependenciesToRepositories(manager, dependencies, (IEnumerable<IPackageRepository>)repositories);
        }

        public static IEnumerable<ICommandResult> CopyResolvedDependenciesToRepositories(this IPackageManager manager, DependencyResolutionResult dependencies, IEnumerable<IPackageRepository> repositoriesToWriteTo)
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
                    .Skip(1).ToList();

                if (repositoriesForDependency.Count == 0)
                    repositoriesForDependency = repositoriesToWriteTo.ToList();

                
                foreach (var repository in repositoriesForDependency.Where(x=>x.CanPublish))
                {
                    yield return new Result("Copying '{0}' to '{1}'", dependency.Package.FullName, repository.Name);
                    manager.UpdateDependency(dependency, repository);
                }
            }
        }
        public static IEnumerable<IPackageRepository> RepositoriesToWriteTo(this IEnvironment environment)
        {
            return environment.RemoteRepositories.Concat(new[]
            {
                environment.CurrentDirectoryRepository,
                environment.SystemRepository,
                environment.ProjectRepository
            });
        }
        public static IEnumerable<IPackageRepository> RepositoriesToReadFrom(this IEnvironment environment)
        {
            return new[]
            {
                environment.CurrentDirectoryRepository, 
                environment.ProjectRepository, 
                environment.SystemRepository
            }.Concat(environment.RemoteRepositories);
        }
    }
}