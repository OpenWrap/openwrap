using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenWrap.Commands.Core;
using OpenWrap.Dependencies;
using OpenWrap.Repositories;
using OpenWrap.Services;

namespace OpenWrap.Commands.Wrap
{
    [Command(Verb = "update", Noun = "wrap")]
    public class UpdateWrapCommand : AbstractCommand
    {

        protected IEnvironment Environment
        {
            get { return WrapServices.GetService<IEnvironment>(); }
        }

        protected IPackageManager PackageManager
        {
            get { return WrapServices.GetService<IPackageManager>(); }
        }

        bool? _system;

        [CommandInput(DisplayName = "System", IsRequired = false, Name="System")]
        public bool System
        {
            get { return _system != null ? (bool)_system : false; }
            set { _system = value; }
        }

        bool? _project;

        [CommandInput(IsRequired = false)]
        public bool Project
        {
            get { return _project == true || (_project == null && _system != true); }
            set { _project = value; }
        }

        public UpdateWrapCommand()
        {
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
                return new GenericError("Project repository not found, cannot update. If you meant to update the system repository, use the -System input.");
            return null;
        }
        IEnumerable<ICommandOutput> UpdateSystemPackages()
        {
            if (!System) yield break;

            yield return new Result("Searching for updated packages...");
            foreach (var packageToSearch in CreateDescriptorForEachSystemPackage())
            {
                var resolveResult = PackageManager.TryResolveDependencies(packageToSearch, Environment.RemoteRepositories.Concat(new[] { Environment.CurrentDirectoryRepository }));

                foreach (var m in PackageManager.CopyPackagesToRepositories(resolveResult, Environment.SystemRepository))
                    yield return m;
                foreach (var m in PackageManager.ExpandPackages(Environment.SystemRepository))
                    yield return m;
            }
        }

        IEnumerable<ICommandOutput> UpdateProjectPackages()
        {
            if (!Project)
                yield break;

            var resolvedPackages = PackageManager.TryResolveDependencies(Environment.Descriptor, Environment.RemoteRepositories.Concat(new[] { Environment.SystemRepository, Environment.CurrentDirectoryRepository }));

            var copyResult = PackageManager.CopyPackagesToRepositories(
                    resolvedPackages,
                    Environment.RepositoriesForWrite()
                    );
            foreach(var m in copyResult) yield return m;

            var expandResult = PackageManager.ExpandPackages(Environment.RepositoriesForWrite().ToArray());
            foreach (var m in expandResult) yield return m;
        }

        IEnumerable<WrapDescriptor> CreateDescriptorForEachSystemPackage()
        {
            
            var installedPackages = Environment.SystemRepository.PackagesByName.Select(x => x.Key);

            return (
                           from systemPackage in Environment.SystemRepository.PackagesByName
                           let systemPackageName = systemPackage.Key
                           let maxPackageVersion = (
                                                           from versionedPackage in systemPackage
                                                           orderby versionedPackage.Version descending
                                                           select versionedPackage.Version
                                                   ).First()
                           select new WrapDescriptor
                           {
                                   Dependencies =
                                           {
                                                   new PackageDependency
                                                   {
                                                           Name = systemPackageName,
                                                           VersionVertices = { new GreaterThenVersionVertex(maxPackageVersion) }
                                                   }
                                           }
                           }
                   ).ToList();
        }
    }
}
