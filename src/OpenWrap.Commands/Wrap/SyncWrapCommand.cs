using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap;
using OpenWrap.Commands;
using OpenWrap.Commands.Core;
using OpenWrap.Dependencies;
using OpenWrap.Repositories;
using OpenWrap.Services;

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
        [CommandInput]
        public bool SystemOnly { get; set; }

        [CommandInput]
        public bool ProjectOnly { get; set; }

        public IEnumerable<ICommandOutput> Execute()
        {
            if (SystemOnly && ProjectOnly)
            {
                yield return new GenericError("Cannot have both SystemOnly and ProjectOnly as parameters, they are mutually exclusive.");
                yield break;
            }
            if (Environment.Descriptor == null)
            {
                yield return new GenericError("Cannot sync when no descriptor file is present.");
                yield break;
            }
            yield return new Result("Synchronizing all packages.");

            // TODO: Check if not in project repository and fail nicely
            var descriptor = Environment.Descriptor;

            var dependencyResolveResult = PackageManager.TryResolveDependencies(descriptor, Environment.RepositoriesForRead());
            var repositoriesToWriteTo = Environment.RemoteRepositories.Concat(Environment.CurrentDirectoryRepository);

            bool all = !SystemOnly && !ProjectOnly;
            if (SystemOnly || all)
                repositoriesToWriteTo = repositoriesToWriteTo.Concat(Environment.SystemRepository);
            if (ProjectOnly || all)
                repositoriesToWriteTo = repositoriesToWriteTo.Concat(Environment.ProjectRepository);


            foreach (var result in PackageManager.CopyPackagesToRepositories(
                dependencyResolveResult, repositoriesToWriteTo))
                yield return result;
        }


    }
}