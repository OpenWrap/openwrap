using System;
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
                return new Error("Project repository not found, cannot update. If you meant to update the system repository, use the -System input.");
            return null;
        }
        IEnumerable<ICommandOutput> UpdateSystemPackages()
        {
            if (!System) yield break;

            yield return new Result("Searching for updated packages...");
            foreach (var packageToSearch in CreateDescriptorForEachSystemPackage())
            {
                var sourceRepos = Environment.RemoteRepositories.Concat(Environment.CurrentDirectoryRepository);

                var resolveResult = PackageManager.TryResolveDependencies(packageToSearch, sourceRepos);

                foreach (var m in PackageManager.CopyPackagesToRepositories(resolveResult, Environment.RemoteRepositories.Concat(Environment.SystemRepository)))
                    if (m is DependencyResolutionFailedResult)
                        yield return PackageNotFoundInRemote(m);

                    else
                        yield return m;
                foreach (var m in VerifyPackageCache(packageToSearch)) yield return m;
            }
        }

        IEnumerable<ICommandOutput> VerifyPackageCache(PackageDescriptor packageDescriptor)
        {
            return PackageManager.VerifyPackageCache(Environment, packageDescriptor);
        }

        IEnumerable<ICommandOutput> UpdateProjectPackages()
        {
            if (!Project)
                yield break;

            var sourceRepos = Environment.RemoteRepositories
                    .Concat(Environment.SystemRepository,
                            Environment.CurrentDirectoryRepository);

            var updateDescriptor = new PackageDescriptor(Environment.Descriptor);
            if (!string.IsNullOrEmpty(Name))
                updateDescriptor.Dependencies = updateDescriptor.Dependencies.Where(x => x.Name.Equals(Name, StringComparison.OrdinalIgnoreCase)).ToList();


            var resolvedPackages = PackageManager.TryResolveDependencies(
                updateDescriptor,
                sourceRepos);

            if (!resolvedPackages.IsSuccess)
                yield return FailedUpdate(resolvedPackages);

            var copyResult = PackageManager.CopyPackagesToRepositories(
                resolvedPackages,
                Environment.RepositoriesForWrite()
                );
            foreach (var m in copyResult) yield return m;

            foreach (var m in PackageManager.VerifyPackageCache(Environment, updateDescriptor)) yield return m;
        }

        ICommandOutput FailedUpdate(DependencyResolutionResult resolvedPackages)
        {
            // try to find conflicting resolutions
            var t = from dependency in resolvedPackages.Dependencies.GroupBy(x => x.Dependency.Name, StringComparer.OrdinalIgnoreCase)
                    where dependency.Count() > 1
                    select dependency;
            if (t.Count() > 0)
                return new DependenciesConflictMessage(t.ToList());

            return new Error("An unkown update error has occured.");
        }

        GenericMessage PackageNotFoundInRemote(ICommandOutput m)
        {
            return new GenericMessage("Package '{0}' doesn't exist in any remote repository.", ((DependencyResolutionFailedResult)m).Result.Dependencies.First().Dependency.Name)
            {
                Type = CommandResultType.Warning
            };
        }

        IEnumerable<PackageDescriptor> CreateDescriptorForEachSystemPackage()
        {


            return (
                           from systemPackage in Environment.SystemRepository.PackagesByName
                           let systemPackageName = systemPackage.Key
                           where ShouldIncludePackageInSystemUpdate(systemPackageName)
                           let maxPackageVersion = (
                                                           from versionedPackage in systemPackage
                                                           orderby versionedPackage.Version descending
                                                           select versionedPackage.Version
                                                   ).First()
                           select new PackageDescriptor
                           {
                               Dependencies =
                                           {
                                                   new PackageDependency
                                                   {
                                                           Name = systemPackageName,
                                                           VersionVertices = { new GreaterThanVersionVertex(maxPackageVersion) }
                                                   }
                                           }
                           }
                   ).ToList();
        }

        bool ShouldIncludePackageInSystemUpdate(string systemPackageName)
        {
            return string.IsNullOrEmpty(Name) ? true : Name.Equals(systemPackageName, StringComparison.OrdinalIgnoreCase);
        }
    }

    internal class DependenciesConflictMessage : Error
    {
        public List<IGrouping<string, ResolvedDependency>> ConflictingPackages { get; set; }

        public DependenciesConflictMessage(List<IGrouping<string, ResolvedDependency>> packageNames)
        {
            ConflictingPackages = packageNames;
            this.Type = CommandResultType.Error;
        }
        public override string ToString()
        {
            return "The following packages have conflicting dependencies:\r\n"
                   + ConflictingPackages.Select(x => 
                       x.Key + " versions: " + x.Select(y=>y.Package.Version.ToString()).Join(", ")
                             ).Join(Environment.NewLine);
        }
    }
}
