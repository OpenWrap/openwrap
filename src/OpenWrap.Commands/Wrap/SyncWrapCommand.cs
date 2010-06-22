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

            var dependencyResolveResult = PackageManager.TryResolveDependencies(descriptor, Environment.RepositoriesToReadFrom());

            foreach (var result in PackageManager
                    .CopyResolvedDependenciesToRepositories(dependencyResolveResult,Environment.RepositoriesToWriteTo()))
                yield return result;
        }


    }
}