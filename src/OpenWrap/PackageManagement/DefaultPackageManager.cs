using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Commands;
using OpenWrap.Dependencies;
using OpenWrap.Repositories;

namespace OpenWrap.PackageManagement
{
    public class DefaultPackageManager : IPackageManager
    {
        readonly IPackageDeployer _deployer;

        readonly IPackageResolver _resolver;

        IPackageExporter _exporter;

        public DefaultPackageManager(IPackageDeployer deployer, IPackageResolver resolver, IPackageExporter exporter)
        {
            _deployer = deployer;
            _resolver = resolver;
            _exporter = exporter;
        }

        public IEnumerable<PackageOperationResult> AddProjectPackage(PackageRequest packageToAdd,
                                                                     IEnumerable<IPackageRepository> sourceRepositories,
                                                                     PackageDescriptor projectDescriptor,
                                                                     IPackageRepository projectRepository,
                                                                     PackageAddOptions options = PackageAddOptions.Default)
        {
            Check.NotNull(packageToAdd, "packageToAdd");
            Check.NoNullElements(sourceRepositories, "sourceRepositories");
            Check.NotNull(projectDescriptor, "projectDescriptor");
            Check.NotNull(projectRepository, "projectRepository");

            return AddProjectPackageCore(packageToAdd, sourceRepositories, projectDescriptor, projectRepository, options);
        }

        IEnumerable<PackageOperationResult> AddProjectPackageCore(PackageRequest packageToAdd, IEnumerable<IPackageRepository> sourceRepositories, PackageDescriptor projectDescriptor, IPackageRepository projectRepository, PackageAddOptions options)
        {
            var finalDescriptor = (options & PackageAddOptions.UpdateDescriptor) == PackageAddOptions.UpdateDescriptor
                                          ? projectDescriptor
                                          : new PackageDescriptor(projectDescriptor);
            var existingEntries = finalDescriptor.Dependencies.Where(x => x.Name.EqualsNoCase(packageToAdd.Name)).ToList();
            if (existingEntries.Count > 0)
            {
                finalDescriptor.Dependencies.RemoveRange(existingEntries);
                yield return new PackageDescriptorUpdateResult(PackageDescriptorDependencyUpdate.Updated);
            }
            else
            {
                yield return new PackageDescriptorUpdateResult(PackageDescriptorDependencyUpdate.Added);
            }

            finalDescriptor.Dependencies.Add(ToDependency(packageToAdd, options));

            foreach (var m in CopyPackageCore(sourceRepositories, new[] { projectRepository }, finalDescriptor, x => true))
                yield return m;
        }

        public IEnumerable<PackageOperationResult> AddSystemPackage(PackageRequest packageToAdd,
                                                                    IEnumerable<IPackageRepository> sourceRepositories,
                                                                    IPackageRepository systemRepository,
                                                                    PackageAddOptions options = PackageAddOptions.Default)
        {
            return CopyPackageCore(sourceRepositories, new[] { systemRepository }, ToDescriptor(packageToAdd, options), x => true);
        }

        public IEnumerable<PackageOperationResult> CleanProjectPackages(PackageDescriptor packages, IPackageRepository projectRepository)
        {
            if (packages == null) throw new ArgumentNullException("packages");
            if (projectRepository == null) throw new ArgumentNullException("projectRepository");

            var repoForClean = projectRepository as ISupportCleaning;
            if (repoForClean == null) throw new ArgumentException("projectRepository must implement ISupportCleaning");
            return CleanProjectPackagesCore(packages, repoForClean, x => true);
        }

        public IEnumerable<PackageOperationResult> CleanProjectPackages(PackageDescriptor packages, IPackageRepository projectRepository, string name)
        {
            if (packages == null) throw new ArgumentNullException("packages");
            if (projectRepository == null) throw new ArgumentNullException("projectRepository");

            var repoForClean = projectRepository as ISupportCleaning;
            if (repoForClean == null) throw new ArgumentException("projectRepository must implement ISupportCleaning");
            return CleanProjectPackagesCore(packages, repoForClean, x => name.EqualsNoCase(x));
        }

        public IEnumerable<PackageOperationResult> CleanSystemPackages(IPackageRepository systemRepository)
        {
            var toClean = systemRepository as ISupportCleaning;
            if (toClean == null) throw new ArgumentException("The repository must implement ISupportCleaning.", "systemRepository");
            return CleanSystemPackagesCore(toClean, x => true);
        }

        public IEnumerable<PackageOperationResult> CleanSystemPackages(IPackageRepository systemRepository, string packageName)
        {
            var toClean = systemRepository as ISupportCleaning;
            if (toClean == null) throw new ArgumentException("The repository must implement ISupportCleaning.", "systemRepository");
            return CleanSystemPackagesCore(toClean, x => packageName.EqualsNoCase(packageName));
        }

        public IEnumerable<PackageOperationResult> RemoveProjectPackage(PackageRequest packageToRemove,
                                                                        PackageDescriptor packageDescriptor,
                                                                        IPackageRepository projectRepository,
                                                                        PackageRemoveOptions options = PackageRemoveOptions.Default)
        {
            if (packageToRemove == null) throw new ArgumentNullException("packageToRemove");
            if (packageDescriptor == null) throw new ArgumentNullException("packageDescriptor");
            if (projectRepository == null) throw new ArgumentNullException("projectRepository");

            return (packageToRemove.ExactVersion == null && !packageToRemove.LastVersion)
                           ? RemoveFromDescriptor(packageToRemove, packageDescriptor, projectRepository, options)
                           : RemovePackageFilesFromProjectRepo(packageToRemove, projectRepository);
        }

        public IEnumerable<PackageOperationResult> RemoveSystemPackage(PackageRequest packageToRemove, IPackageRepository systemRepository, PackageRemoveOptions options = PackageRemoveOptions.Default)
        {
            if (packageToRemove == null) throw new ArgumentNullException("packageToRemove");
            if (systemRepository == null) throw new ArgumentNullException("systemRepository");
            return RemovePackageFromRepository(packageToRemove, systemRepository);
        }

        public IEnumerable<PackageOperationResult> UpdateProjectPackages(IEnumerable<IPackageRepository> sourceRepositories, IPackageRepository projectRepository, PackageDescriptor projectDescriptor)
        {
            if (sourceRepositories == null) throw new ArgumentNullException("sourceRepositories");
            if (projectRepository == null) throw new ArgumentNullException("projectRepository");
            if (projectDescriptor == null) throw new ArgumentNullException("projectDescriptor");
            if (sourceRepositories.Any(x => x == null)) throw new ArgumentException("Some repositories are null.", "sourceRepositories");

            return CopyPackageCore(sourceRepositories, new[] { projectRepository }, projectDescriptor, x => true);
        }

        public IEnumerable<PackageOperationResult> UpdateProjectPackages(IEnumerable<IPackageRepository> sourceRepositories,
                                                                         IPackageRepository projectRepository,
                                                                         PackageDescriptor projectDescriptor,
                                                                         string packageName)
        {
            if (sourceRepositories == null) throw new ArgumentNullException("sourceRepositories");
            if (projectRepository == null) throw new ArgumentNullException("projectRepository");
            if (projectDescriptor == null) throw new ArgumentNullException("projectDescriptor");
            if (sourceRepositories.Any(x => x == null)) throw new ArgumentException("Some repositories are null.", "sourceRepositories");

            return CopyPackageCore(sourceRepositories, new[] { projectRepository }, projectDescriptor, x => x.EqualsNoCase(packageName));
        }

        public IEnumerable<PackageOperationResult> UpdateSystemPackages(IEnumerable<IPackageRepository> sourceRepositories, IPackageRepository systemRepository)
        {
            return UpdateSystemPackageCore(sourceRepositories, systemRepository, x => true);
        }

        public IEnumerable<PackageOperationResult> UpdateSystemPackages(IEnumerable<IPackageRepository> sourceRepositories, IPackageRepository systemRepository, string packageName)
        {
            return UpdateSystemPackageCore(sourceRepositories, systemRepository, x => x.EqualsNoCase(packageName));
        }

        static IEnumerable<PackageAnchoredResult> AnchorPackages(DependencyResolutionResult resolvedPackages, IEnumerable<IPackageRepository> destinationRepositories)
        {
            return from repo in destinationRepositories.OfType<ISupportAnchoring>()
                   from successfulPackage in resolvedPackages.SuccessfulPackages
                   where successfulPackage.IsAnchored
                   let packageInstances = from packageInstance in successfulPackage.Packages
                                          where packageInstance != null &&
                                                packageInstance.Source == repo
                                          select packageInstance
                   from anchorResult in repo.AnchorPackages(packageInstances)
                   select anchorResult;
        }

        static IPackageInfo GetBestSourcePackage(IEnumerable<IPackageRepository> sourceRepositories, IEnumerable<IPackageInfo> packages)
        {
            return (
                           from repo in sourceRepositories
                           let compatiblePackage = packages.FirstOrDefault(x => x.Source == repo)
                           where compatiblePackage != null
                           select compatiblePackage
                   )
                   .First();
        }

        static IEnumerable<PackageOperationResult> RemovePackageFilesFromProjectRepo(PackageRequest packageToRemove, IPackageRepository projectRepository)
        {
            return RemovePackageFromRepository(packageToRemove, projectRepository);
        }

        static IEnumerable<PackageOperationResult> RemovePackageFromRepository(PackageRequest packageToRemove, IPackageRepository repository)
        {
            var versionToRemove = packageToRemove.LastVersion
                                          ? repository.PackagesByName[packageToRemove.Name].Select(x => x.Version)
                                                    .OrderByDescending(_ => _)
                                                    .FirstOrDefault()
                                          : packageToRemove.ExactVersion;
            var packagesToKeep = from package in repository.PackagesByName.SelectMany(_ => _)
                                 let matchesName = package.Name.EqualsNoCase(packageToRemove.Name)
                                 let matchesVersion = versionToRemove == null ? true : package.Version == versionToRemove
                                 where !(matchesName && matchesVersion)
                                 select package;
            return ((ISupportCleaning)repository).Clean(packagesToKeep).Cast<PackageOperationResult>();
        }

        static ICollection<VersionVertex> ToVersionVertices(PackageRequest packageToRequest)
        {
            var vertices = new List<VersionVertex>();
            if (packageToRequest.ExactVersion != null)
                vertices.Add(new ExactVersionVertex(packageToRequest.ExactVersion));
            if (packageToRequest.MinVersion != null)
                vertices.Add(new GreaterThenOrEqualVersionVertex(packageToRequest.MinVersion));
            if (packageToRequest.MaxVersion != null)
                vertices.Add(new LessThanVersionVertex(packageToRequest.MaxVersion));
            if (packageToRequest.ExactVersion == null && packageToRequest.MinVersion == null && packageToRequest.MaxVersion == null)
                vertices.Add(new AnyVersionVertex());
            return vertices;
        }

        IEnumerable<PackageOperationResult> CleanProjectPackagesCore(PackageDescriptor projectDescriptor, ISupportCleaning projectRepository, Func<string, bool> packageName)
        {
            var resolvedPackages = _resolver.TryResolveDependencies(projectDescriptor, new[] { projectRepository });
            if(resolvedPackages.SuccessfulPackages.Any() == false)
            {
                yield return new PackageCleanCannotDo(projectDescriptor);
                yield break;
            }
            var projectPackagesInUse = from successfulPackageStack in resolvedPackages.SuccessfulPackages
                                       from package in successfulPackageStack.Packages
                                       where packageName(package.Identifier.Name)
                                       select package;

            var otherPackages = from packagesByName in projectRepository.PackagesByName
                                where !packageName(packagesByName.Key)
                                from package in packagesByName
                                select package;
            var packagesInUse = projectPackagesInUse.Concat(otherPackages).ToList();


            foreach (var cleanedPackage in projectRepository.Clean(packagesInUse))
                yield return cleanedPackage;
            foreach (var anchored in AnchorPackages(resolvedPackages, new[] { projectRepository }))
                yield return anchored;
        }

        IEnumerable<PackageOperationResult> CleanSystemPackagesCore(ISupportCleaning systemRepository, Func<string, bool> packageNameSelector)
        {
            var lastVersions = from packageByName in systemRepository.PackagesByName
                               where packageNameSelector(packageByName.Key)
                               select packageByName.OrderByDescending(x => x.Version).First();

            var untouchedVersions = systemRepository.PackagesByName.Where(x => !packageNameSelector(x.Key)).SelectMany(x => x);

            foreach (var clean in systemRepository.Clean(lastVersions.Concat(untouchedVersions)))
                yield return clean;
        }

        IEnumerable<PackageOperationResult> CopyPackageCore(IEnumerable<IPackageRepository> sourceRepositories,
                                                            IEnumerable<IPackageRepository> destinationRepositories,
                                                            PackageDescriptor descriptor,
                                                            Func<string, bool> nameSelector)
        {
            var updateDescriptor = new PackageDescriptor(descriptor);
            updateDescriptor.Dependencies.Clear();
            updateDescriptor.Dependencies.AddRange(descriptor.Dependencies.Where(x => nameSelector(x.Name)));

            var resolvedPackages = _resolver.TryResolveDependencies(
                    updateDescriptor,
                    sourceRepositories);

            if (!resolvedPackages.IsSuccess)
            {
                foreach (var packageResolution in ReturnError(resolvedPackages))
                    yield return packageResolution;
                yield break;
            }

            var packagesForGacDetection = resolvedPackages.SuccessfulPackages.Select(x => x.Packages.First()).ToList();

            foreach (var conflict in from errors in GacResolver.InGac(packagesForGacDetection)
                                     select new PackageGacConflictResult(errors.Key, errors))
                yield return conflict;

            foreach (var m in CopyPackagesToRepositories(sourceRepositories, resolvedPackages, destinationRepositories))
                yield return m;


            foreach (var repo in destinationRepositories)
                repo.RefreshPackages();

            // need to refresh the resolve with the newly copied packages
            resolvedPackages = _resolver.TryResolveDependencies(
                    updateDescriptor,
                    destinationRepositories);

            foreach(var anchor in AnchorPackages(resolvedPackages, destinationRepositories))
                yield return anchor;
        }

        IEnumerable<PackageOperationResult> CopyPackagesToRepositories(IEnumerable<IPackageRepository> sourceRepositories,
                                                                       DependencyResolutionResult resolvedPackages,
                                                                       IEnumerable<IPackageRepository> destinationRepositories)
        {
            var publishingRepos = destinationRepositories.NotNull().OfType<ISupportPublishing>().ToList();
            foreach (var foundPackage in resolvedPackages.SuccessfulPackages)
            {
                foreach (var repository in publishingRepos)
                {
                    var existingUpToDateVersion = repository.PackagesByName.Contains(foundPackage.Identifier.Name)
                                                          ? repository.PackagesByName[foundPackage.Identifier.Name]
                                                                    .Where(x => foundPackage.Identifier.Version != null && x.Version >= foundPackage.Identifier.Version)
                                                                    .OrderByDescending(x => x.Version)
                                                                    .FirstOrDefault()
                                                          : null;
                    if (existingUpToDateVersion == null)
                    {
                        var sourcePackage = GetBestSourcePackage(sourceRepositories, foundPackage.Packages);

                        _deployer.DeployDependency(sourcePackage, repository);
                        yield return new PackagePublishedResult(sourcePackage, repository);
                    }
                    else
                    {
                        yield return new PackageUpToDateResult(existingUpToDateVersion, repository);
                    }
                }
            }
            foreach (var repo in publishingRepos)
                repo.PublishCompleted();
        }
        public IEnumerable<PackageOperationResult> ListPackages(IEnumerable<IPackageRepository> repositories, string query = null)
        {
            var packages = repositories.SelectMany(x => x.PackagesByName.NotNull());
            if (query != null)
            {
                var queryRegex = query.Wildcard(true);
                packages = packages.Where(x => queryRegex.IsMatch(x.Key));
            }
            foreach(var x in packages)
                yield return new PackageFoundResult(x);
        }

        IEnumerable<PackageDescriptor> CreateDescriptorForEachSystemPackage(IPackageRepository repository, Func<string, bool> packageNameSelection)
        {
            return (
                           from systemPackage in repository.PackagesByName
                           let systemPackageName = systemPackage.Key
                           where packageNameSelection(systemPackageName)
                           let maxPackageVersion = (
                                                           from versionedPackage in systemPackage
                                                           orderby versionedPackage.Version descending
                                                           select versionedPackage.Version
                                                   ).First()
                           select new PackageDescriptor
                           {
                               Dependencies =
                                           {
                                                   new PackageDependencyBuilder(systemPackageName)
                                                    .VersionVertex(new UpdatePackageVertex(maxPackageVersion))
                                           }
                           }
                   ).ToList();
        }

        PackageOperationResult PackageConflict(ResolvedPackage resolvedPackage)
        {
            return new PackageConflictResult(resolvedPackage);
        }

        PackageOperationResult PackageMissing(ResolvedPackage resolvedPackage)
        {
            return new PackageMissingResult(resolvedPackage);
        }

        IEnumerable<PackageOperationResult> RemoveFromDescriptor(PackageRequest packageToRemove, PackageDescriptor packageDescriptor, IPackageRepository projectRepository, PackageRemoveOptions options)
        {
            var dependency = packageDescriptor.Dependencies.FirstOrDefault(x => x.Name.EqualsNoCase(packageToRemove.Name));
            if (dependency == null)
            {
                yield return new PackageDescriptorUpdateResult(PackageDescriptorDependencyUpdate.DependencyNotFound);
                yield break;
            }
            packageDescriptor.Dependencies.Remove(dependency);
            if ((options & PackageRemoveOptions.Clean) == PackageRemoveOptions.Clean)
                foreach (var cleaned in CleanProjectPackages(packageDescriptor, projectRepository, packageToRemove.Name))
                    yield return cleaned;
        }

        IEnumerable<PackageOperationResult> ReturnError(DependencyResolutionResult resolvedPackages)
        {
            return resolvedPackages.ConflictingPackages.Select(PackageConflict)
                    .Concat(resolvedPackages.MissingPackages.Select(PackageMissing));
        }

        PackageDependency ToDependency(PackageRequest packageToAdd, PackageAddOptions options)
        {
            return new PackageDependencyBuilder(packageToAdd.Name)
                    .SetVersionVertices(ToVersionVertices(packageToAdd))
                    .Anchored((options & PackageAddOptions.Anchor) == PackageAddOptions.Anchor)
                    .Content((options & PackageAddOptions.Content) == PackageAddOptions.Content);
        }

        PackageDescriptor ToDescriptor(PackageRequest package, PackageAddOptions options)
        {
            return new PackageDescriptor
            {
                Dependencies =
                            {
                                    new PackageDependencyBuilder(package.Name)
                                        .Content((options & PackageAddOptions.Content) == PackageAddOptions.Content)
                                        .Anchored((options & PackageAddOptions.Anchor) == PackageAddOptions.Anchor)
                                        .SetVersionVertices(ToVersionVertices(package))
                            }
            };
        }

        IEnumerable<PackageOperationResult> UpdateSystemPackageCore(IEnumerable<IPackageRepository> sourceRepositories, IPackageRepository systemRepository, Func<string, bool> packageNameSelector)
        {
            return CreateDescriptorForEachSystemPackage(systemRepository, packageNameSelector)
                    .SelectMany(x => CopyPackageCore(sourceRepositories, new[] { systemRepository }, x, name => true));
        }

        class UpdatePackageVertex : VersionVertex
        {
            public UpdatePackageVertex(Version existingVersion)
                : base(existingVersion)
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
    }


    internal class PackageCleanCannotDo : PackageOperationResult
    {
        public PackageCleanCannotDo(PackageDescriptor projectDescriptor)
        {
            
        }

        public override bool Success
        {
            get { return false; }
        }

        public override ICommandOutput ToOutput()
        {
            return new Error("Cannot clean package as it wasn't found.");
        }
    }
}