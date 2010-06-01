using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Build.Services;
using OpenWrap.Commands;
using OpenWrap.Commands.Core;
using OpenWrap.Dependencies;
using OpenWrap.Repositories;

namespace OpenWrap.Commands.Wrap
{
    [Command(Verb = "sync", Noun = "wrap")]
    public class SyncWrapCommand : ICommand
    {
        protected IEnvironment Environment
        {
            get { return WrapServices.GetService<IEnvironment>(); }
        }

        protected IPackageManager PackageManager
        {
            get { return WrapServices.GetService<IPackageManager>(); }
        }

        public IEnumerable<ICommandResult> Execute()
        {
            yield return new Result("Synchronizing all packages.");

            // TODO: Check if not in project repository and fail nicely
            var descriptor = Environment.Descriptor;

            var dependencyResolveResult = PackageManager.TryResolveDependencies(descriptor, Environment.ProjectRepository, Environment.UserRepository, Environment.RemoteRepositories);

            if (!dependencyResolveResult.IsSuccess)
            {
                yield return DependencyResolutionFailed(dependencyResolveResult);
                yield break;
            }
            foreach(var dependency in dependencyResolveResult.Dependencies.Where(x => Environment.RemoteRepositories.Any(rem => rem == x.Package.Source)))
            {
                yield return new Result("Copying {0} to user repository.", dependency.Package.FullName);
                using (var packageStream = dependency.Package.Load().OpenStream())
                {
                    if (Environment.ProjectRepository != null)
                    {
                        var package = Environment.UserRepository.Publish(dependency.Package.FullName, packageStream);
                        using (var localPackageStream = package.Load().OpenStream())
                        {
                            yield return new Result("Copying {0} to project repository.", package.Name);
                            Environment.ProjectRepository.Publish(dependency.Package.FullName, localPackageStream);
                        }
                    }
                }
            }
            foreach (var localDependency in dependencyResolveResult.Dependencies.Where(x => x.Package.Source == Environment.UserRepository))
            {
                var package = localDependency.Package;
                using (var packageStream = package.Load().OpenStream())
                {
                    yield return new Result("Copying {0} to project repository.", package.Name);
                    Environment.ProjectRepository.Publish(package.FullName, packageStream);
                }
            }
        }


        ICommandResult DependencyResolutionFailed(DependencyResolutionResult result)
        {
            return new Result("Dependency resolution failed.")
            {
                Success = false
            };
        }
    }
}