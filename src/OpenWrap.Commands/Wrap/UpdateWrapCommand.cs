using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap;
using OpenWrap.PackageManagement;
using OpenWrap.PackageManagement.DependencyResolvers;

namespace OpenWrap.Commands.Wrap
{
    [Command(Verb = "update", Noun = "wrap")]
    public class UpdateWrapCommand : WrapCommand
    {
        bool? _project;
        bool? _system;

        [CommandInput(Position = 0)]
        public string Name { get; set; }

        [CommandInput]
        public bool Project
        {
            get { return _project == true || (_project == null && _system != true); }
            set { _project = value; }
        }

        [CommandInput]
        public bool System
        {
            get { return _system != null ? (bool)_system : false; }
            set { _system = value; }
        }
        protected override IEnumerable<Func<IEnumerable<ICommandOutput>>> Validators()
        {
            yield return VerifyInputs;
        }

        protected override IEnumerable<ICommandOutput> ExecuteCore()
        {
            var update = Enumerable.Empty<ICommandOutput>();
            if (Project)
                update = update.Concat(UpdateProjectPackages());
            if (System)
                update = update.Concat(UpdateSystemPackages());
            return update;
        }

        protected override ICommandOutput ToOutput(PackageOperationResult packageOperationResult)
        {
            if (packageOperationResult is PackageMissingResult)
                return new Warning("Cannot update package because of missing packages: {0}.", ((PackageMissingResult)packageOperationResult).Package.Identifier);
            else return base.ToOutput(packageOperationResult);
        }

        IEnumerable<ICommandOutput> UpdateProjectPackages()
        {
            yield return new Result("Updating project packages...");

            var sourceRepos = new[] { HostEnvironment.CurrentDirectoryRepository, HostEnvironment.SystemRepository }.Concat(HostEnvironment.RemoteRepositories);
            var errors = new List<PackageOperationResult>();
            foreach (var x in (string.IsNullOrEmpty(Name)
                                       ? PackageManager.UpdateProjectPackages(sourceRepos, HostEnvironment.ProjectRepository, HostEnvironment.Descriptor)
                                       : PackageManager.UpdateProjectPackages(sourceRepos, HostEnvironment.ProjectRepository, HostEnvironment.Descriptor, Name)))
                if (x is PackageMissingResult || x is PackageConflictResult) errors.Add(x);
                else yield return x.ToOutput();

            foreach (var failed in errors.GetFailures())
            {

                var errorMessage = "{0} could not be updated\r\n{1}";

                var details = new[] { " - " + failed.First() }.Concat(failed.Skip(1).Select(x => "   " + x));

                yield return new Error(errorMessage, failed.Key, details.Join(@"\r\n"));
            }
        }

        IEnumerable<ICommandOutput> UpdateSystemPackages()
        {
            var isByName = !string.IsNullOrEmpty(Name);
            yield return new Result(isByName
                                            ? "Updating system packages..."
                                            : string.Format("Updating system package '{0}'", Name));



            var sourceRepos = new[] { HostEnvironment.CurrentDirectoryRepository }.Concat(HostEnvironment.RemoteRepositories).ToList();

            var errors = new List<PackageOperationResult>();

            foreach (var x in (isByName ? PackageManager.UpdateSystemPackages(sourceRepos, HostEnvironment.SystemRepository, Name)
                                        : PackageManager.UpdateSystemPackages(sourceRepos, HostEnvironment.SystemRepository)))
                if (x is PackageMissingResult || x is PackageConflictResult) errors.Add(x);
                else yield return x.ToOutput();
            var stacks = errors.GetFailures();
            foreach (var failed in stacks)
            {
                var key = failed.Key;
                var errorTraces = failed.SelectMany(_=>new[] { " - " + _.First() }.Concat(_.Skip(1).Select(x => "   " + x))).Join("\r\n");
                var errorMessage = "{0} could not be updated:\r\n{1}";

                if (isByName)
                    yield return new Error(errorMessage, failed.Key, errorTraces);
                else
                    yield return new Warning(errorMessage, failed.Key, errorTraces);
            }
        }

        IEnumerable<ICommandOutput> VerifyInputs()
        {
            if (Project && HostEnvironment.ProjectRepository == null)
                yield return new Error("Project repository not found, cannot update. If you meant to update the system repository, use the -System input.");
        }
    }
}