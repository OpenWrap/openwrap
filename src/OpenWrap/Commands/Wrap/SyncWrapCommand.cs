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
    [Command(Verb = "sync", Namespace = "wrap")]
    public class SyncWrapCommand : ICommand
    {
        public IEnumerable<ICommandResult> Execute()
        {
            yield return new Result("Synchronizing all packages.");
            var packageManager = WrapServices.GetService<IPackageManager>();
            var environment = WrapServices.GetService<IEnvironment>();

            var descriptor = environment.Descriptor;
            var dependencyResolveResult = packageManager.TryResolveDependencies(descriptor, environment.ProjectRepository, environment.UserRepository, environment.RemoteRepositories);

            if (!dependencyResolveResult.IsSuccess)
            {
                yield return DependencyResolutionFailed(dependencyResolveResult);
                yield break;
            }
            foreach(var dependency in dependencyResolveResult.Dependencies.Where(x => environment.RemoteRepositories.Any(rem => rem == x.Package.Source)))
            {
                yield return new Result("Copying {0} to user repository.", dependency.Package.Name);
                using (var packageStream = dependency.Package.Load().OpenStream())
                {
                    var packageFileName = dependency.Package.Name + "-" + dependency.Package.Version;
                    var package = environment.UserRepository.Publish(packageFileName, packageStream);
                    using (var localPackageStream = package.Load().OpenStream())
                    {
                        yield return new Result("Copying {0} to project repository.", package.Name);
                        environment.ProjectRepository.Publish(packageFileName, localPackageStream);
                    }
                }
            }
            foreach (var localDependency in dependencyResolveResult.Dependencies.Where(x => x.Package.Source == environment.UserRepository))
            {
                var package = localDependency.Package;
                var packageFileName = package.Name + "-" + package.Version;
                using (var packageStream = package.Load().OpenStream())
                {
                    yield return new Result("Copying {0} to project repository.", package.Name);
                    environment.ProjectRepository.Publish(packageFileName, packageStream);
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