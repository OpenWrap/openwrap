using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Collections;
using OpenWrap.PackageManagement.DependencyResolvers;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;

namespace OpenWrap.PackageManagement
{
    public class DefaultPackageManager : IPackageManager
    {
        readonly IPackageDeployer _deployer;

        readonly IPackageResolver _resolver;

        public DefaultPackageManager(IPackageDeployer deployer, IPackageResolver resolver)
        {
            _deployer = deployer;
            _resolver = resolver;
        }

        public IPackageAddResult AddProjectPackage(PackageRequest packageToAdd, IEnumerable<IPackageRepository> sourceRepositories, IPackageDescriptor projectDescriptor, IPackageRepository projectRepository, PackageAddOptions options = PackageAddOptions.Default)
        {
            Check.NotNull(packageToAdd, "packageToAdd");
            Check.NoNullElements(sourceRepositories, "sourceRepositories");
            Check.NotNull(projectDescriptor, "projectDescriptor");
            Check.NotNull(projectRepository, "projectRepository");

            return new PackageAddResultIterator(AddProjectPackageCore(packageToAdd, sourceRepositories, projectDescriptor, projectRepository, options));
        }

        public IPackageAddResult AddSystemPackage(PackageRequest packageToAdd,
                                                  IEnumerable<IPackageRepository> sourceRepositories,
                                                  IPackageRepository systemRepository,
                                                  PackageAddOptions options = PackageAddOptions.Default)
        {
            return new PackageAddResultIterator(CopyPackageCore(sourceRepositories, new[] { systemRepository }, ToDescriptor(packageToAdd, options), x => true));
        }

        public IPackageCleanResult CleanProjectPackages(IPackageDescriptor packages, IPackageRepository projectRepository, PackageCleanOptions options = PackageCleanOptions.Default)
        {
            if (packages == null) throw new ArgumentNullException("packages");
            if (projectRepository == null) throw new ArgumentNullException("projectRepository");

            var repoForClean = projectRepository as ISupportCleaning;
            if (repoForClean == null) throw new ArgumentException("projectRepository must implement ISupportCleaning");
            return new PackageCleanResultIterator(CleanProjectPackagesCore(packages, repoForClean, x => true));
        }

        public IPackageCleanResult CleanProjectPackages(IPackageDescriptor packages, IPackageRepository projectRepository, string name, PackageCleanOptions options = PackageCleanOptions.Default)
        {
            if (packages == null) throw new ArgumentNullException("packages");
            if (projectRepository == null) throw new ArgumentNullException("projectRepository");

            var repoForClean = projectRepository as ISupportCleaning;
            if (repoForClean == null) throw new ArgumentException("projectRepository must implement ISupportCleaning");
            return new PackageCleanResultIterator(CleanProjectPackagesCore(packages, repoForClean, x => name.EqualsNoCase(x)));
        }

        public IPackageCleanResult CleanSystemPackages(IPackageRepository systemRepository, PackageCleanOptions options = PackageCleanOptions.Default)
        {
            var toClean = systemRepository as ISupportCleaning;
            if (toClean == null) throw new ArgumentException("The repository must implement ISupportCleaning.", "systemRepository");
            return new PackageCleanResultIterator(CleanSystemPackagesCore(toClean, x => true));
        }

        public IPackageCleanResult CleanSystemPackages(IPackageRepository systemRepository, string packageName, PackageCleanOptions options = PackageCleanOptions.Default)
        {
            var toClean = systemRepository as ISupportCleaning;
            if (toClean == null) throw new ArgumentException("The repository must implement ISupportCleaning.", "systemRepository");
            return new PackageCleanResultIterator(CleanSystemPackagesCore(toClean, x => packageName.EqualsNoCase(packageName)));
        }

        public IPackageListResult ListPackages(IEnumerable<IPackageRepository> repositories, string query = null, PackageListOptions options = PackageListOptions.Default)
        {
            return new PackageListResultIterator(ListPackagesCore(repositories, query));
        }

        public IPackageRemoveResult RemoveProjectPackage(PackageRequest packageToRemove, IPackageDescriptor packageDescriptor, IPackageRepository projectRepository, PackageRemoveOptions options = PackageRemoveOptions.Default)
        {
            if (packageToRemove == null) throw new ArgumentNullException("packageToRemove");
            if (packageDescriptor == null) throw new ArgumentNullException("packageDescriptor");
            if (projectRepository == null) throw new ArgumentNullException("projectRepository");

            return new PackageRemoveResultIterator((packageToRemove.ExactVersion == null && !packageToRemove.LastVersion)
                                                           ? RemoveFromDescriptor(packageToRemove, packageDescriptor, projectRepository, options)
                                                           : RemovePackageFilesFromProjectRepo(packageToRemove, projectRepository));
        }

        public IPackageRemoveResult RemoveSystemPackage(PackageRequest packageToRemove, IPackageRepository systemRepository, PackageRemoveOptions options = PackageRemoveOptions.Default)
        {
            if (packageToRemove == null) throw new ArgumentNullException("packageToRemove");
            if (systemRepository == null) throw new ArgumentNullException("systemRepository");
            return new PackageRemoveResultIterator(RemovePackageFromRepository(packageToRemove, systemRepository));
        }

        public IPackageUpdateResult UpdateProjectPackages(IEnumerable<IPackageRepository> sourceRepositories, IPackageRepository projectRepository, IPackageDescriptor projectDescriptor, PackageUpdateOptions options = PackageUpdateOptions.Recurse)
        {
            if (sourceRepositories == null) throw new ArgumentNullException("sourceRepositories");
            if (projectRepository == null) throw new ArgumentNullException("projectRepository");
            if (projectDescriptor == null) throw new ArgumentNullException("projectDescriptor");
            if (sourceRepositories.Any(x => x == null)) throw new ArgumentException("Some repositories are null.", "sourceRepositories");

            return new PackageUpdateResultIterator(CopyPackageCore(sourceRepositories, new[] { projectRepository }, projectDescriptor, x => true));
        }

        public IPackageUpdateResult UpdateProjectPackages(IEnumerable<IPackageRepository> sourceRepositories,
                                                          IPackageRepository projectRepository,
                                                          IPackageDescriptor projectDescriptor,
                                                          string packageName,
                                                          PackageUpdateOptions options = PackageUpdateOptions.Default)
        {
            if (sourceRepositories == null) throw new ArgumentNullException("sourceRepositories");
            if (projectRepository == null) throw new ArgumentNullException("projectRepository");
            if (projectDescriptor == null) throw new ArgumentNullException("projectDescriptor");
            if (sourceRepositories.Any(x => x == null)) throw new ArgumentException("Some repositories are null.", "sourceRepositories");

            return new PackageUpdateResultIterator(CopyPackageCore(sourceRepositories, new[] { projectRepository }, projectDescriptor, x => x.EqualsNoCase(packageName)));
        }

        public IPackageUpdateResult UpdateSystemPackages(IEnumerable<IPackageRepository> sourceRepositories,
                                                         IPackageRepository systemRepository,
                                                         PackageUpdateOptions options = PackageUpdateOptions.Default)
        {
            return new PackageUpdateResultIterator(UpdateSystemPackageCore(sourceRepositories, systemRepository, x => true));
        }

        public IPackageUpdateResult UpdateSystemPackages(IEnumerable<IPackageRepository> sourceRepositories,
                                                         IPackageRepository systemRepository,
                                                         string packageName,
                                                         PackageUpdateOptions options = PackageUpdateOptions.Default)
        {
            return new PackageUpdateResultIterator(UpdateSystemPackageCore(sourceRepositories, systemRepository, x => x.EqualsNoCase(packageName)));
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

        static IEnumerable<PackageDescriptor> CreateDescriptorForEachSystemPackage(IPackageRepository repository, Func<string, bool> packageNameSelection)
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

        static IPackageInfo GetExistingPackage(ISupportPublishing destinationRepository, ResolvedPackage foundPackage, Func<Version, bool> versionSelector)
        {
            return destinationRepository.PackagesByName.Contains(foundPackage.Identifier.Name)
                           ? destinationRepository.PackagesByName[foundPackage.Identifier.Name]
                                     .Where(x => foundPackage.Identifier.Version != null && versionSelector(x.Version))
                                     .OrderByDescending(x => x.Version)
                                     .FirstOrDefault()
                           : null;
        }

        static IEnumerable<PackageOperationResult> ListPackagesCore(IEnumerable<IPackageRepository> repositories, string query)
        {
            var packages = repositories.SelectMany(x => x.PackagesByName.NotNull());
            if (query != null)
            {
                var queryRegex = query.Wildcard(true);
                packages = packages.Where(x => queryRegex.IsMatch(x.Key));
            }
            foreach (var x in packages)
                yield return new PackageFoundResult(x);
        }

        static PackageOperationResult PackageConflict(ResolvedPackage resolvedPackage)
        {
            return new PackageConflictResult(resolvedPackage);
        }

        static PackageOperationResult PackageMissing(ResolvedPackage resolvedPackage)
        {
            return new PackageMissingResult(resolvedPackage);
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

        static PackageDependency ToDependency(PackageRequest packageToAdd, PackageAddOptions options)
        {
            return new PackageDependencyBuilder(packageToAdd.Name)
                    .SetVersionVertices(ToVersionVertices(packageToAdd))
                    .Anchored((options & PackageAddOptions.Anchor) == PackageAddOptions.Anchor)
                    .Content((options & PackageAddOptions.Content) == PackageAddOptions.Content);
        }

        static IPackageDescriptor ToDescriptor(PackageRequest package, PackageAddOptions options)
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

        static IEnumerable<VersionVertex> ToVersionVertices(PackageRequest packageToRequest)
        {
            var vertices = new List<VersionVertex>();
            if (packageToRequest.ExactVersion != null)
                vertices.Add(new EqualVersionVertex(packageToRequest.ExactVersion));
            if (packageToRequest.MinVersion != null)
                vertices.Add(new GreaterThanOrEqualVersionVertex(packageToRequest.MinVersion));
            if (packageToRequest.MaxVersion != null)
                vertices.Add(new LessThanVersionVertex(packageToRequest.MaxVersion));
            if (packageToRequest.ExactVersion == null && packageToRequest.MinVersion == null && packageToRequest.MaxVersion == null)
                vertices.Add(new AnyVersionVertex());
            return vertices;
        }

        IEnumerable<PackageOperationResult> AddProjectPackageCore(PackageRequest packageToAdd,
                                                                  IEnumerable<IPackageRepository> sourceRepositories,
                                                                  IPackageDescriptor projectDescriptor,
                                                                  IPackageRepository projectRepository,
                                                                  PackageAddOptions options)
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

        IEnumerable<PackageOperationResult> CleanProjectPackagesCore(IPackageDescriptor projectDescriptor, ISupportCleaning projectRepository, Func<string, bool> packageName)
        {
            var resolvedPackages = _resolver.TryResolveDependencies(projectDescriptor, new[] { projectRepository });
            if (resolvedPackages.SuccessfulPackages.Any() == false)
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
                                                            IPackageDescriptor descriptor,
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

            foreach (var anchor in AnchorPackages(resolvedPackages, destinationRepositories))
                yield return anchor;
        }

        IEnumerable<PackageOperationResult> CopyPackagesToRepositories(IEnumerable<IPackageRepository> sourceRepositories,
                                                                       DependencyResolutionResult resolvedPackages,
                                                                       IEnumerable<IPackageRepository> destinationRepositories)
        {
            var publishingRepos = destinationRepositories.NotNull().OfType<ISupportPublishing>().ToList();
            foreach (var destinationRepository in publishingRepos)
            {
                using (var publisher = destinationRepository.Publisher())
                {
                    foreach (var foundPackage in resolvedPackages.SuccessfulPackages)
                    {
                        var existingUpToDateVersion = GetExistingPackage(destinationRepository, foundPackage, x => x >= foundPackage.Identifier.Version);
                        if (existingUpToDateVersion == null)
                        {
                            var sourcePackage = GetBestSourcePackage(sourceRepositories, foundPackage.Packages);

                            _deployer.DeployDependency(sourcePackage, publisher);
                            var existingVersion = GetExistingPackage(destinationRepository, foundPackage, x => x < foundPackage.Identifier.Version);

                            yield return existingVersion == null
                                                 ? new PackageAddedResult(sourcePackage, destinationRepository)
                                                 : new PackageUpdatedResult(existingVersion, sourcePackage, destinationRepository);
                        }
                        else
                        {
                            yield return new PackageUpToDateResult(existingUpToDateVersion, destinationRepository);
                        }
                    }
                }
            }
        }

        IEnumerable<PackageOperationResult> RemoveFromDescriptor(PackageRequest packageToRemove, IPackageDescriptor packageDescriptor, IPackageRepository projectRepository, PackageRemoveOptions options)
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
            return resolvedPackages.DiscardedPackages.Select(PackageConflict)
                    .Concat(resolvedPackages.MissingPackages.Select(PackageMissing));
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
}