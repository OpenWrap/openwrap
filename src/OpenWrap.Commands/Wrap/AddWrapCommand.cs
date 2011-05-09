using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Collections;
using OpenWrap.PackageManagement;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;
using SysPath = System.IO.Path;

namespace OpenWrap.Commands.Wrap
{
    [Command(Verb = "add", Noun = "wrap")]
    public class AddWrapCommand : WrapCommand
    {
        bool _packageNotFound;
        bool? _project;

        bool? _system;
        FolderRepository _userSpecifiedRepository;

        [CommandInput]
        public bool Anchored { get; set; }

        [CommandInput]
        public bool Content { get; set; }

        [CommandInput]
        public string From { get; set; }

        [CommandInput]
        public string MaxVersion { get; set; }

        [CommandInput]
        public string MinVersion { get; set; }

        [CommandInput(IsRequired = true, Position = 0)]
        public string Name { get; set; }

        [CommandInput]
        public bool NoDescriptorUpdate { get; set; }

        [CommandInput]
        public bool NoHooks { get; set; }

        [CommandInput]
        public bool Project
        {
            get { return _project ?? (_system == null); }
            set { _project = value; }
        }

        [CommandInput]
        public string Scope { get; set; }

        [CommandInput]
        public bool System
        {
            get { return _system ?? _project == null; }
            set { _system = value; }
        }

        [CommandInput(Position = 1)]
        public string Version { get; set; }

        PackageAddOptions AddOptions
        {
            get
            {
                PackageAddOptions addOptions = 0;
                if (Anchored)
                    addOptions |= PackageAddOptions.Anchor;
                if (Content)
                    addOptions |= PackageAddOptions.Content;
                if (!NoDescriptorUpdate)
                    addOptions |= PackageAddOptions.UpdateDescriptor;
                if (!NoHooks)
                    addOptions |= PackageAddOptions.Hooks;
                return addOptions;
            }
        }

        PackageRequest PackageRequest
        {
            get
            {
                if (Version != null) return PackageRequest.Exact(Name, Version.ToVersion());
                if (MinVersion != null || MaxVersion != null) return PackageRequest.Between(Name, MinVersion.ToVersion(), MaxVersion.ToVersion());
                return PackageRequest.Any(Name);
            }
        }


        bool ShouldUpdateDescriptor
        {
            get
            {
                return NoDescriptorUpdate == false &&
                       HostEnvironment.Descriptor != null;
            }
        }

        protected override IEnumerable<ICommandOutput> ExecuteCore()
        {
            var targetDescriptor = HostEnvironment.GetOrCreateScopedDescriptor(Scope ?? string.Empty);

            yield return VerifyDescriptor(targetDescriptor);
            yield return VerifyProjectRepository();

            yield return SetupEnvironmentForAdd();
            var sourceRepositories = GetSourceRepositories();


            if (Project && System)
            {
                var sysToAdd = new List<PackageIdentifier>();
                using (ChangeMonitor(targetDescriptor))
                {
                    foreach (var m in PackageManager.AddProjectPackage(PackageRequest, sourceRepositories, targetDescriptor.Value, HostEnvironment.ProjectRepository, AddOptions))
                    {
                        yield return ToOutput(m);
                        ParseSuccess(m, sysToAdd.Add);
                    }
                    foreach (var identifier in sysToAdd)
                        foreach (var m in PackageManager.AddSystemPackage(PackageRequest.Exact(identifier.Name, identifier.Version), sourceRepositories, HostEnvironment.SystemRepository))
                            yield return ToOutput(m);
                }
            }
            else if (Project)
            {
                using (ChangeMonitor(targetDescriptor))
                {
                    foreach (var m in PackageManager.AddProjectPackage(PackageRequest, sourceRepositories, targetDescriptor.Value, HostEnvironment.ProjectRepository, AddOptions))
                    {
                        yield return ToOutput(m);
                    }
                }
            }
            else if (System)
            {
                foreach (var m in PackageManager.AddSystemPackage(PackageRequest, sourceRepositories, HostEnvironment.SystemRepository, AddOptions))
                {
                    yield return ToOutput(m);
                }
            }

            if (_packageNotFound)
            {
                var hit = false;
                foreach (var m in PackageManager.ListPackages(sourceRepositories, Name))
                {
                    if (!hit)
                    {
                        yield return new Info("Did you mean one of the following packages?", Name);
                        hit = true;
                    }
                    yield return ToOutput(m);
                }
            }
            if (ShouldUpdateDescriptor)
                TrySaveDescriptorFile(targetDescriptor);
        }

        protected override ICommandOutput ToOutput(PackageOperationResult packageOperationResult)
        {
            if (packageOperationResult is PackageMissingResult)
                _packageNotFound = true;
            return base.ToOutput(packageOperationResult);
        }

        protected override IEnumerable<Func<IEnumerable<ICommandOutput>>> Validators()
        {
            yield return ValidateInputs;
        }

        static void ParseSuccess(PackageOperationResult m, Action<PackageIdentifier> onSuccess, Action onFailure = null)
        {
            var id = m is PackageUpdatedResult
                             ? ((PackageUpdatedResult)m).Package.Identifier
                             : (m is PackageAddedResult ? ((PackageAddedResult)m).Package.Identifier : null);
            if (m.Success && id != null)
                onSuccess(id);
            else if (m.Success)
                return;
            else if (onFailure != null)
                onFailure();
        }


        IEnumerable<IPackageRepository> GetSourceRepositories()
        {
            return new[] { _userSpecifiedRepository, HostEnvironment.CurrentDirectoryRepository, HostEnvironment.SystemRepository }
                    .Concat(GetFetchRepositories())
                    .Concat(HostEnvironment.ProjectRepository)
                    .NotNull();
        }

        ICommandOutput SetupEnvironmentForAdd()
        {
            var directory = HostEnvironment.CurrentDirectory;

            var fromDirectory = string.IsNullOrEmpty(From) ? null : FileSystem.GetDirectory(From);
            if (fromDirectory != null && fromDirectory.Exists)
            {
                if (SysPath.GetExtension(Name).EqualsNoCase(".wrap") &&
                    SysPath.IsPathRooted(Name) &&
                    FileSystem.GetDirectory(SysPath.GetDirectoryName(Name)) != fromDirectory)
                {
                    return new Error("You provided both -From and -Name, but -Name is a path. Try removing the -From parameter.");
                }
                directory = fromDirectory;
                _userSpecifiedRepository = new FolderRepository(directory, FolderRepositoryOptions.Default);
            }


            if (SysPath.GetExtension(Name).EqualsNoCase(".wrap") && directory.GetFile(SysPath.GetFileName(Name)).Exists)
            {
                var originalName = Name;
                Name = PackageNameUtility.GetName(SysPath.GetFileNameWithoutExtension(Name));
                Version = PackageNameUtility.GetVersion(SysPath.GetFileNameWithoutExtension(originalName)).ToString();

                return new Warning("The requested package contained '.wrap' in the name. Assuming you pointed to a file name and meant a package named '{0}' with version qualifier '{1}'.",
                                   Name,
                                   Version);
            }
            return null;
        }


        IEnumerable<ICommandOutput> ValidateInputs()
        {
            var gotVersion = Version != null;
            var gotMinVersion = MinVersion != null;
            var gotMaxVersion = MaxVersion != null;
            var numberOfVersionInputTypes = (new[] { gotVersion, (gotMinVersion || gotMaxVersion) }).Count(v => v);

            if (numberOfVersionInputTypes > 1)
            {
                yield return new Error("Arguments for 'version' and 'version boundaries' cannot be combined.");
                yield break;
            }

            if (gotVersion && Version.ToVersion() == null)
            {
                yield return new Error("Could not parse version: " + Version);
                yield break;
            }

            if (gotMinVersion && MinVersion.ToVersion() == null)
            {
                yield return new Error("Could not parse minversion: " + MinVersion);
                yield break;
            }

            if (gotMaxVersion && MaxVersion.ToVersion() == null)
            {
                yield return new Error("Could not parse maxversion: " + MaxVersion);
                yield break;
            }

            if (Project && HostEnvironment.ProjectRepository == null)
            {
                yield return new Error("Project repository doesn't exist but -project has been specified.");
                yield break;
            }
            if (!string.IsNullOrEmpty(Scope) && !Project)
            {
                yield return new Error("Cannot specify a scope when adding to a system package.");
                yield break;
            }
        }


        ICommandOutput VerifyDescriptor(FileBased<IPackageDescriptor> descriptor)
        {
            if (NoDescriptorUpdate)
                return new GenericMessage("Descriptor file will not be updated.");
            return descriptor.File.Exists
                           ? new GenericMessage(@"Using descriptor {0}.", descriptor.File.Name)
                           : new GenericMessage("Creating descriptor {0}.", descriptor.File.Name);
        }

        ICommandOutput VerifyProjectRepository()
        {
            return HostEnvironment.ProjectRepository != null
                           ? new GenericMessage("Project repository present.")
                           : new GenericMessage("Project repository absent.");
        }
    }
}