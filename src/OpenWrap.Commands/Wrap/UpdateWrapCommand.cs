using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.PackageManagement;
using OpenWrap.PackageManagement.DependencyResolvers;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;

namespace OpenWrap.Commands.Wrap
{
    [Command(Verb = "update", Noun = "wrap")]
    public class UpdateWrapCommand : WrapCommand
    {
        bool? _project;
        IEnumerable<IPackageRepository> _remoteRepositories;
        bool? _system;
        FileBased<IPackageDescriptor> _targetDescriptor;

        [CommandInput]
        public string From { get; set; }

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
            get { return _system != null && (bool)_system; }
            set { _system = value; }
        }

        [CommandInput]
        public string Scope { get; set; }
        protected override IEnumerable<ICommandOutput> ExecuteCore()
        {
            var update = Enumerable.Empty<ICommandOutput>();
            if (Project)
                update = update.Concat(UpdateProjectPackages());
            if (System)
                update = update.Concat(UpdateSystemPackages());
            return update;
        }

        protected override IEnumerable<Func<IEnumerable<ICommandOutput>>> Validators()
        {
            yield return ProjectExistsWhenProjectFlagSpecified;
            yield return SetRemoteRepositories;
            yield return AddUserSpecifiedRepository;
        }

        IEnumerable<ICommandOutput> AddUserSpecifiedRepository()
        {
            if (From != null)
            {
                var remoteDir = FileSystem.GetDirectory(From);
                if (remoteDir.Exists)
                {
                    _remoteRepositories = new[] { new FolderRepository(remoteDir) }.Concat(_remoteRepositories).ToList();
                }
                else
                {
                    yield return new Error("The -From input is not recognized as an existing directory.");
                }
            }
        }

        IEnumerable<ICommandOutput> ProjectExistsWhenProjectFlagSpecified()
        {
            if (Project && HostEnvironment.ProjectRepository == null)
                yield return new NotInProject();
        }

        IEnumerable<ICommandOutput> SetRemoteRepositories()
        {
            _remoteRepositories = new[] { HostEnvironment.CurrentDirectoryRepository, HostEnvironment.SystemRepository }
                .Concat(Remotes.FetchRepositories());
            if (HostEnvironment.ProjectRepository != null)
                _remoteRepositories = _remoteRepositories.Concat(new[] { HostEnvironment.ProjectRepository });
            yield break;
        }

        IEnumerable<ICommandOutput> UpdateProjectPackages()
        {
            yield return new Info("Updating project packages...");

            var sourceRepos = _remoteRepositories;
            var errors = new List<PackageOperationResult>();
            bool updated = false;

            _targetDescriptor = HostEnvironment.GetOrCreateScopedDescriptor(Scope ?? string.Empty);
            var packageDescriptor = _targetDescriptor.Value.Lock(HostEnvironment.ProjectRepository);

            foreach (var x in (string.IsNullOrEmpty(Name)
                                   ? PackageManager.UpdateProjectPackages(sourceRepos, HostEnvironment.ProjectRepository, packageDescriptor)
                                   : PackageManager.UpdateProjectPackages(sourceRepos, HostEnvironment.ProjectRepository, packageDescriptor, Name)))
                if (x is PackageMissingResult || x is PackageConflictResult)
                {
                    errors.Add(x);
                }
                else
                {
                    updated = true;
                    yield return x.ToOutput();
                }
            if (updated)
                _targetDescriptor.File.Touch();

            foreach (var failed in errors.GetFailures())
            {
                var errorMessage = "{0} could not be updated\r\n{1}";

                var details = failed.SelectMany(_ => new[] { " - " + _.First() }.Concat(_.Skip(1).Select(x => "   " + x)));


                yield return new Error(errorMessage, failed.Key, details.JoinString(@"\r\n"));
            }
        }

        IEnumerable<ICommandOutput> UpdateSystemPackages()
        {
            var isByName = !string.IsNullOrEmpty(Name);
            yield return new Info(!isByName
                                      ? "Updating system packages..."
                                      : string.Format("Updating system package '{0}'", Name));


            var sourceRepos = _remoteRepositories;

            var errors = new List<PackageOperationResult>();

            foreach (var x in (isByName
                                   ? PackageManager.UpdateSystemPackages(sourceRepos, HostEnvironment.SystemRepository, Name)
                                   : PackageManager.UpdateSystemPackages(sourceRepos, HostEnvironment.SystemRepository)))
                if (x is PackageMissingResult || x is PackageConflictResult) errors.Add(x);
                else yield return x.ToOutput();
            var stacks = errors.GetFailures();
            foreach (var failed in stacks)
            {
                var key = failed.Key;
                var errorTraces = failed.SelectMany(_ => new[] { " - " + _.First() }.Concat(_.Skip(1).Select(x => "   " + x))).JoinString("\r\n");
                var errorMessage = "{0} could not be updated:\r\n{1}";

                if (isByName)
                    yield return new Error(errorMessage, failed.Key, errorTraces);
                else
                    yield return new Warning(errorMessage, failed.Key, errorTraces);
            }
        }
    }
}