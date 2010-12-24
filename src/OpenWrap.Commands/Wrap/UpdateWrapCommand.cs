using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenWrap.Commands.Core;
using OpenWrap.PackageManagement;
using OpenWrap.PackageManagement.DependencyResolvers;
using OpenWrap.Repositories;
using OpenWrap.Services;
using OpenWrap;
namespace OpenWrap.Commands.Wrap
{
    [Command(Verb = "update", Noun = "wrap")]
    public class UpdateWrapCommand : WrapCommand
    {
        bool? _system;

        [CommandInput(Position = 0)]
        public string Name { get; set; }

        [CommandInput]
        public bool System
        {
            get { return _system != null ? (bool)_system : false; }
            set { _system = value; }
        }

        bool? _project;

        [CommandInput]
        public bool Project
        {
            get { return _project == true || (_project == null && _system != true); }
            set { _project = value; }
        }

        public override IEnumerable<ICommandOutput> Execute()
        {
            var update = Enumerable.Empty<ICommandOutput>();
            if (Project)
                update = update.Concat(UpdateProjectPackages());
            if (System)
                update = update.Concat(UpdateSystemPackages());
            return Either(VerifyInputs)
                    .Or(update);
        }
        ICommandOutput VerifyInputs()
        {
            if (Project && Environment.ProjectRepository == null)
                return new Error("Project repository not found, cannot update. If you meant to update the system repository, use the -System input.");
            return null;
        }
        IEnumerable<ICommandOutput> UpdateSystemPackages()
        {
            if (!System) yield break;

            yield return new Result("Searching for updated packages...");


            var sourceRepos = new[] { Environment.CurrentDirectoryRepository }.Concat(Environment.RemoteRepositories).ToList();

            foreach (var x in (string.IsNullOrEmpty(Name)
                ? PackageManager.UpdateSystemPackages(sourceRepos, Environment.SystemRepository)
                : PackageManager.UpdateSystemPackages(sourceRepos, Environment.SystemRepository, Name))
                .Select(ToOutput))
                yield return x;
        }

        IEnumerable<ICommandOutput> UpdateProjectPackages()
        {
            if (!Project)
                yield break;

            var sourceRepos = new[] { Environment.CurrentDirectoryRepository, Environment.SystemRepository }.Concat(Environment.RemoteRepositories);
            foreach (var x in (string.IsNullOrEmpty(Name) ? PackageManager.UpdateProjectPackages(sourceRepos, Environment.ProjectRepository, Environment.Descriptor)
                                                         : PackageManager.UpdateProjectPackages(sourceRepos, Environment.ProjectRepository, Environment.Descriptor, Name))
                    .Select(ToOutputForProject))
                yield return x;
        }
         ICommandOutput ToOutputForProject(PackageOperationResult packageOperationResult)
         {
             return packageOperationResult is PackageMissingResult
                            ? new PackageMissingOutput((PackageMissingResult)packageOperationResult)
                            : ToOutput(packageOperationResult);
         }
        protected override ICommandOutput ToOutput(PackageOperationResult packageOperationResult)
        {
            return packageOperationResult is PackageMissingResult
                            ? null
                            : base.ToOutput(packageOperationResult);
        }
    }

}
