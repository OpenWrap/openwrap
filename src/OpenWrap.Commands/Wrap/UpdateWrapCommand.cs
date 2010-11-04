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
                var sourceRepos = Environment.RemoteRepositories.Concat(Environment.CurrentDirectoryRepository).ToList();

                var resolveResult = PackageResolver.TryResolveDependencies(packageToSearch, sourceRepos);
                var successful = resolveResult.ResolvedPackages.Where(x => x.Package != null).ToList();
                resolveResult = new DependencyResolutionResult { IsSuccess = successful.Count > 0, ResolvedPackages = successful };
                if (!resolveResult.IsSuccess)
                    continue;
                foreach (var m in PackageResolver.CopyPackagesToRepositories(resolveResult, Environment.SystemRepository))
                    if (m is DependencyResolutionFailedResult)
                        yield return PackageNotFoundInRemote((DependencyResolutionFailedResult)m);

                    else
                        yield return m;
                foreach (var m in VerifyPackageCache(packageToSearch)) yield return m;
            }
        }

        IEnumerable<ICommandOutput> VerifyPackageCache(PackageDescriptor packageDescriptor)
        {
            return PackageResolver.VerifyPackageCache(Environment, packageDescriptor);
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


            var resolvedPackages = PackageResolver.TryResolveDependencies(
                updateDescriptor,
                sourceRepos);

            if (!resolvedPackages.IsSuccess)

            {
                foreach (var m in FailedUpdate(resolvedPackages, sourceRepos)) yield return m;
                yield break;
            }

            foreach (var m in resolvedPackages.GacConflicts(Environment.ExecutionEnvironment))
                yield return m;

            var copyResult = PackageResolver.CopyPackagesToRepositories(
                resolvedPackages,
                Environment.ProjectRepository
                );
            foreach (var m in copyResult) yield return m;

            foreach (var m in PackageResolver.VerifyPackageCache(Environment, updateDescriptor)) yield return m;
        }

        IEnumerable<ICommandOutput> FailedUpdate(DependencyResolutionResult resolvedPackages, IEnumerable<IPackageRepository> sourceRepos)
        {
            foreach(var notFoundPackage in resolvedPackages.ResolvedPackages.Where(x=>x.Package == null))
                yield return new DependenciesNotFoundInRepositories(notFoundPackage.Dependencies, sourceRepos);

            var conflictingDependencies = resolvedPackages.ResolvedPackages
                    .Where(x => x.Package != null)
                    .GroupBy(x => x.Package.Name, StringComparer.OrdinalIgnoreCase)
                    .Where(x => x.Count() > 1);
                    
            if (conflictingDependencies.Count() > 0)
                yield return new DependenciesConflictMessage(conflictingDependencies.ToList());
        }

        GenericMessage PackageNotFoundInRemote(DependencyResolutionFailedResult m)
        {
            return new GenericMessage("Package '{0}' doesn't exist in any remote repository.", m.Result.ResolvedPackages.First().Dependencies.Select(x=>x.Dependency.Name).First())
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
                                                           VersionVertices = { new UpdatePackageVertex(maxPackageVersion) }
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

    public class UpdatePackageVertex : VersionVertex
    {
        public UpdatePackageVertex(Version existingVersion) : base(existingVersion)
        {
        }
        public override bool IsCompatibleWith(Version version)
        {
            return (Version.Major == version.Major
                    && Version.Minor == version.Minor
                    && Version.Build == version.Build
                    && Version.Revision < version.Revision)
                   || 
                   (Version.Major == version.Major
                    && Version.Minor == version.Minor
                    && Version.Build < version.Build)
                   ||
                   (Version.Major == version.Major
                    && Version.Minor < version.Minor)
                   ||
                   (Version.Major < version.Major);
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
            return string.Format("{0} not found in '{1}'.",Dependencies.Select(x=>"'" + x.Dependency.Name + "'").Distinct().Join(", "), Repositories.Select(x => x.Name).Join(", "));
        }
    }
}
