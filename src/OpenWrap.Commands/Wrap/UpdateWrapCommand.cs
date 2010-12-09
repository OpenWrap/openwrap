using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenWrap.Commands.Core;
using OpenWrap.Dependencies;
using OpenWrap.PackageManagement;
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
                    .Select(ToOutput))
                yield return x;
        }
        protected override ICommandOutput ToOutput(PackageOperationResult packageOperationResult)
        {
            return packageOperationResult is PackageMissingResult
                            ? new Warning("Package {0} not found in repositories.", ((PackageMissingResult)packageOperationResult).Package.Identifier)
                            : base.ToOutput(packageOperationResult);
        }
    }

    public class DependenciesNotFoundInRepositories : Warning
    {
        public IEnumerable<ParentedDependency> Dependencies { get; set; }
        public IEnumerable<IPackageRepository> Repositories { get; set; }

        public DependenciesNotFoundInRepositories(IEnumerable<ParentedDependency> dependencies, IEnumerable<IPackageRepository> repositories)
        {
            Dependencies = dependencies;
            Repositories = repositories;
        }
        public override string ToString()
        {
            return string.Format("{0} not found in '{1}'.", Dependencies.Select(x => "'" + x.Dependency.Name + "'").Distinct().Join(", "), Repositories.Select(x => x.Name).Join(", "));
        }
    }
}
