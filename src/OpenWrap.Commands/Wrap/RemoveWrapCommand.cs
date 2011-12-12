using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.PackageManagement;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;

namespace OpenWrap.Commands.Wrap
{
    [Command(Verb = "remove", Noun = "wrap")]
    public class RemoveWrapCommand : WrapCommand
    {
        bool? _last;
        bool? _project;
        FileBased<IPackageDescriptor> _targetDescriptor;

        [CommandInput]
        public bool Clean { get; set; }

        [CommandInput]
        public bool Last
        {
            get { return _last ?? false; }
            set { _last = value; }
        }

        [CommandInput(IsRequired = true, Position = 0)]
        public string Name { get; set; }

        [CommandInput]
        public bool Project
        {
            get { return _project ?? !System; }
            set { _project = value; }
        }

        [CommandInput]
        public string Scope { get; set; }

        [CommandInput]
        public bool System { get; set; }

        [CommandInput]
        public SemanticVersion Version { get; set; }


        protected PackageRemoveOptions Options
        {
            get
            {
                var options = PackageRemoveOptions.Default;
                //if (Clean)
                //    options |= PackageRemoveOptions.Clean;
                return options;
            }
        }

        protected PackageRequest PackageRequest
        {
            get
            {
                if (Last)
                    return PackageRequest.Last(Name);
                if (Version != null)
                    return PackageRequest.Exact(Name, Version);
                return PackageRequest.Any(Name);
            }
        }

        protected override IEnumerable<ICommandOutput> ExecuteCore()
        {
            var options = Options;
            if (Project && Version != null)
                yield return
                        new Warning(
                                "Because you have selected a specific version to remove from the project repository, your descriptor file will not be updated. To remove the dependency from your descriptor, either do not specify a version or use the set-wrap command to update the version of a package in use by your project.")
                        ;
            

            if (Project)
            {
                var lockedRepo = HostEnvironment.ProjectRepository.Feature<ISupportLocking>();
                
                
                var packageDescriptor = _targetDescriptor.Value;
                if (lockedRepo != null)
                {
                    if (lockedRepo.LockedPackages[string.Empty].Any(_=>_.Name.EqualsNoCase(Name)))
                    {
                        yield return new PackageLockedNotRemoved(Name);
                        yield break;
                    }
                    packageDescriptor = packageDescriptor.Lock(HostEnvironment.ProjectRepository);
                }
                using (SaveDescriptorOnSuccess(_targetDescriptor))
                {
                    foreach (var m in PackageManager.RemoveProjectPackage(PackageRequest, packageDescriptor, HostEnvironment.ProjectRepository, options)) 
                        yield return ToOutput(m);
                }
            }
            if (System)
                foreach (var m in PackageManager.RemoveSystemPackage(PackageRequest, HostEnvironment.SystemRepository, Options)) yield return ToOutput(m);
        }
        protected override ICommandOutput ToOutput(PackageOperationResult packageOperationResult)
        {
            if (Version == null)
                If<PackageDependencyRemovedResult>(packageOperationResult, 
                    _ => _targetDescriptor.Value.Dependencies.Remove(_.Dependency));
            return base.ToOutput(packageOperationResult);
        }
        protected override IEnumerable<Func<IEnumerable<ICommandOutput>>> Validators()
        {
            yield return VerifyInputs;
            yield return InProject;
        }

        IEnumerable<ICommandOutput> VerifyInputs()
        {
            if (Version != null && Last)
                yield return new Error("Cannot use '-Last' and '-Version' together.");
            if (System && !HostEnvironment.SystemRepository.PackagesByName[Name].Any())
                yield return new Error("Cannot find package named '{0}' in system repository.", Name);
        }

        IEnumerable<ICommandOutput> InProject()
        {
            if (!Project) yield break;

            if (HostEnvironment.ProjectRepository != null)
                _targetDescriptor = HostEnvironment.GetOrCreateScopedDescriptor(Scope);
            else
                yield return new Error("Not in a package directory.");
        }
    }
    public class PackageLockedNotRemoved : Error
    {
        public string PackageName { get; set; }

        public PackageLockedNotRemoved(string packageName) : base("Cannont remove package '{0}' as it is currently locked. Unlock the package with 'unlock-wrap {0}' first, then remove.", packageName)
        {
            PackageName = packageName;
        }
    }
}