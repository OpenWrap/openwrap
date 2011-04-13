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
            if (Project && HostEnvironment.ProjectRepository == null)
                return new Error("Project repository not found, cannot update. If you meant to update the system repository, use the -System input.");
            return null;
        }
        IEnumerable<ICommandOutput> UpdateSystemPackages()
        {
            yield return new Result("Updating system packages...");


            var sourceRepos = new[] { HostEnvironment.CurrentDirectoryRepository }.Concat(HostEnvironment.RemoteRepositories).ToList();

            var missings = new List<PackageMissingResult>();
            var conflicts = new List<PackageConflictResult>();

            foreach (var x in (string.IsNullOrEmpty(Name)
                ? PackageManager.UpdateSystemPackages(sourceRepos, HostEnvironment.SystemRepository)
                : PackageManager.UpdateSystemPackages(sourceRepos, HostEnvironment.SystemRepository, Name)))
                if (x is PackageMissingResult) missings.Add((PackageMissingResult)x);
                else if (x is PackageConflictResult) conflicts.Add((PackageConflictResult)x);
                else yield return x.ToOutput();
            var stacks = (from missing in missings
                          from trace in missing.Package.DependencyStacks
                          select new
                          {
                              who = trace.First(),
                              value = string.Format("not found: {0} (trace: {1})",
                                                    missing.Package.Identifier.Name,
                                                    trace)
                          }).Union(
                                  from conflict in conflicts
                                  from trace in conflict.Package.DependencyStacks
                                  select new
                                  {
                                      who = trace.First(),
                                      value = string.Format("conflict: {0} (trace: {1})",
                                                            conflict.Package.Identifier.Name,
                                                            trace)
                                  }
                    ).ToLookup(x => x.who, x => x.value);
            foreach (var failed in stacks)
            {
                yield return new Warning("{0} could not be updated:\r\n\t{1}", failed.Key,
                    failed.Join("\r\n\t"));
            }
        }

        IEnumerable<ICommandOutput> UpdateProjectPackages()
        {
            yield return new Result("Updating project packages...");

            var sourceRepos = new[] { HostEnvironment.CurrentDirectoryRepository, HostEnvironment.SystemRepository }.Concat(HostEnvironment.RemoteRepositories);
            var operation = string.IsNullOrEmpty(Name)
                ? PackageManager.UpdateProjectPackages(sourceRepos, HostEnvironment.ProjectRepository, HostEnvironment.Descriptor)
                : PackageManager.UpdateProjectPackages(sourceRepos, HostEnvironment.ProjectRepository, HostEnvironment.Descriptor, Name);

            //List<PackageMissingResult> missing = new List<PackageMissingResult>();
            //var incompatible = new List<PackageMissingResult>();
            foreach (var x in operation)
                yield return x.ToOutput();//If<PackageMissingResult>(_=>new Warning("{0}: Dependency not found: {1}.", _.Package.DependencyStacks.));
        }
        ICommandOutput ToOutputForProject(PackageOperationResult packageOperationResult)
        {
            return packageOperationResult is PackageMissingResult
                           ? new PackageMissingOutput((PackageMissingResult)packageOperationResult)
                           : ToOutput(packageOperationResult);
        }
        protected override ICommandOutput ToOutput(PackageOperationResult packageOperationResult)
        {
            if (packageOperationResult is PackageMissingResult)
                return new Warning("Cannot update package because of missing packages: {0}.", ((PackageMissingResult)packageOperationResult).Package.Identifier);
            else return base.ToOutput(packageOperationResult);
        }
    }

}
