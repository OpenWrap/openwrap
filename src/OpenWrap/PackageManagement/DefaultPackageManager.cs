using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using OpenWrap.Collections;
using OpenWrap.PackageManagement.DependencyResolvers;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;
using OpenWrap.Runtime;

namespace OpenWrap.PackageManagement
{
    public class DefaultPackageManager : IPackageManager
    {
        readonly IPackageDeployer _deployer;

        readonly IPackageResolver _resolver;
        readonly IPackageExporter _exporter;
        readonly HooksStore _hooks = new HooksStore();


        public IEnumerable<IPackageInfo> ListProjectPackages(IPackageDescriptor descriptor, IPackageRepository projectRepository)
        {
            Check.NotNull(projectRepository, "projectRepository");
            Check.NotNull(descriptor, "descriptor");

            return _resolver.TryResolveDependencies(descriptor, new[] { projectRepository }).SuccessfulPackages.Select(_ => _.Packages.First());
        }

        public IEnumerable<IGrouping<string, TItem>> GetSystemExports<TItem>(IPackageRepository systemRepository, ExecutionEnvironment environment) where TItem : IExportItem
        {
            var packages = systemRepository.PackagesByName.Select(x => x.OrderByDescending(_ => _.SemanticVersion).First());
            return packages.SelectMany(x => _exporter.Exports<TItem>(x.Load(), environment));
        }
        public IEnumerable<IGrouping<string, TItem>> GetProjectExports<TItem>(IPackageDescriptor descriptor, IPackageRepository projectRepository, ExecutionEnvironment environment) where TItem : IExportItem
        {
            return ListProjectPackages(descriptor, projectRepository).SelectMany(x => _exporter.Exports<TItem>(x.Load(), environment));
        }

        public event PackageUpdated PackageUpdated
        {
            add { _hooks.PackageUpdated += value; }
            remove { _hooks.PackageUpdated -= value; }
        }
        public event PackageChanged PackageAdded
        {
            add { _hooks.PackageAdded += value; }
            remove { _hooks.PackageAdded -= value; }
        }
        public event PackageChanged PackageRemoved
        {
            add { _hooks.PackageRemoved += value; }
            remove { _hooks.PackageRemoved -= value; }
        }

        public DefaultPackageManager(IPackageDeployer deployer, IPackageResolver resolver, IPackageExporter exporter)
        {
            _deployer = deployer;
            _exporter = exporter;
            _resolver = resolver;
        }
        public IPackageAddResult AddProjectPackage(PackageRequest packageToAdd,
                                                   IEnumerable<IPackageRepository> sourceRepositories,
                                                   IPackageDescriptor projectDescriptor,
                                                   IPackageRepository projectRepository,
                                                   PackageAddOptions options = PackageAddOptions.Default)
        {
            Check.NotNull(packageToAdd, "packageToAdd");
            Check.NoNullElements(sourceRepositories, "sourceRepositories");
            Check.NotNull(projectDescriptor, "projectDescriptor");
            Check.NotNull(projectRepository, "projectRepository");

            var result = AddProjectPackageCore(packageToAdd, sourceRepositories, projectDescriptor, projectRepository, options);
            if ((options & PackageAddOptions.Hooks) == PackageAddOptions.Hooks && _hooks != null)
                result = WrapWithHooks(result, projectDescriptor, projectRepository, "project");
            return new PackageAddResultIterator(result);
        }

        IEnumerable<PackageOperationResult> WrapWithHooks(IEnumerable<PackageOperationResult> result, IPackageDescriptor descriptor, IPackageRepository destinationRepository, string repositoryType)
        {
            Func<IEnumerable<IPackageInfo>> currentPackageFactory = () =>
            {
                destinationRepository.RefreshPackages();
                return GetSelectedPackages(_resolver.TryResolveDependencies(descriptor, new[] { destinationRepository })).ToList();
            };
            var currentPackages = currentPackageFactory();
            return new HookedPackageOperationResults(repositoryType, result, _hooks, () => currentPackages, currentPackageFactory);
        }
        
        public IPackageAddResult AddSystemPackage(PackageRequest packageToAdd,
                                                  IEnumerable<IPackageRepository> sourceRepositories,
                                                  IPackageRepository systemRepository,
                                                  PackageAddOptions options = PackageAddOptions.Default)
        {
            Check.NotNull(packageToAdd, "packageToAdd");
            Check.NoNullElements(sourceRepositories, "sourceRepositories");
            Check.NotNull(systemRepository, "systemRepository");

            var returnValue = AddSystemPackageCore(sourceRepositories, systemRepository, packageToAdd, options);
            if ((options & PackageAddOptions.Hooks) == PackageAddOptions.Hooks && _hooks != null)
                returnValue = WrapWithHooks(returnValue, ToDescriptor(packageToAdd, options), systemRepository, "system");
            return new PackageAddResultIterator(returnValue);
        }

        public IPackageCleanResult CleanProjectPackages(IEnumerable<IPackageDescriptor> projectDescriptors, IPackageRepository projectRepository, PackageCleanOptions options = PackageCleanOptions.Default)
        {
            Check.NoNullElements(projectDescriptors, "projectDescriptors");
            if (projectRepository == null) throw new ArgumentNullException("Repository");

            if (projectRepository.Feature<ISupportCleaning>() == null) throw new ArgumentException("projectRepository must implement ISupportCleaning");
            return new PackageCleanResultIterator(CleanProjectPackagesCore(projectDescriptors, projectRepository, x => true));
        }

        public IPackageCleanResult CleanProjectPackages(IEnumerable<IPackageDescriptor> projectDescriptors, IPackageRepository projectRepository, string name, PackageCleanOptions options = PackageCleanOptions.Default)
        {
            Check.NoNullElements(projectDescriptors, "projectDescriptors");

            if (projectRepository == null) throw new ArgumentNullException("projectRepository");

            if (projectRepository.Feature<ISupportCleaning>() == null) throw new ArgumentException("projectRepository must implement ISupportCleaning");
            return new PackageCleanResultIterator(CleanProjectPackagesCore(projectDescriptors, projectRepository, x => name.EqualsNoCase(x)));
        }

        public IPackageCleanResult CleanSystemPackages(IPackageRepository systemRepository, PackageCleanOptions options = PackageCleanOptions.Default)
        {
            if (systemRepository.Feature<ISupportCleaning>() == null) throw new ArgumentException("The repository must implement ISupportCleaning.", "systemRepository");
            return new PackageCleanResultIterator(CleanSystemPackagesCore(systemRepository, x => true));
        }

        public IPackageCleanResult CleanSystemPackages(IPackageRepository systemRepository, string packageName, PackageCleanOptions options = PackageCleanOptions.Default)
        {
            if (systemRepository.Feature<ISupportCleaning>() == null) throw new ArgumentException("The repository must implement ISupportCleaning.", "systemRepository");
            return new PackageCleanResultIterator(CleanSystemPackagesCore(systemRepository, x => packageName.EqualsNoCase(packageName)));
        }

        public IPackageListResult ListPackages(IEnumerable<IPackageRepository> repositories, string query = null, PackageListOptions options = PackageListOptions.Default)
        {
            return new PackageListResultIterator(ListPackagesCore(repositories, query));
        }

        public IPackageRemoveResult RemoveProjectPackage(PackageRequest packageToRemove,
                                                         IPackageDescriptor projectDescriptor,
                                                         IPackageRepository projectRepository,
                                                         PackageRemoveOptions options = PackageRemoveOptions.Default)
        {
            if (packageToRemove == null) throw new ArgumentNullException("packageToRemove");
            if (projectDescriptor == null) throw new ArgumentNullException("projectDescriptor");
            if (projectRepository == null) throw new ArgumentNullException("projectRepository");

            var resultIterator = RemoveProjectPackageCore(packageToRemove, projectDescriptor, projectRepository, options);
            if ((options & PackageRemoveOptions.Hooks) == PackageRemoveOptions.Hooks && _hooks != null)
                resultIterator = WrapWithHooks(resultIterator, projectDescriptor, projectRepository, "project");
            return new PackageRemoveResultIterator(resultIterator);
        }

        public IPackageRemoveResult RemoveSystemPackage(PackageRequest packageToRemove, IPackageRepository systemRepository, PackageRemoveOptions options = PackageRemoveOptions.Default)
        {
            if (packageToRemove == null) throw new ArgumentNullException("packageToRemove");
            if (systemRepository == null) throw new ArgumentNullException("systemRepository");
            var resultIterator = RemoveSystemPackageCore(packageToRemove, systemRepository);

            return new PackageRemoveResultIterator(resultIterator);
        }

        public IPackageUpdateResult UpdateProjectPackages(IEnumerable<IPackageRepository> sourceRepositories,
                                                          IPackageRepository projectRepository,
                                                          IPackageDescriptor projectDescriptor,
                                                          PackageUpdateOptions options = PackageUpdateOptions.Recurse)
        {
            if (sourceRepositories == null) throw new ArgumentNullException("sourceRepositories");
            if (projectRepository == null) throw new ArgumentNullException("projectRepository");
            if (projectDescriptor == null) throw new ArgumentNullException("projectDescriptor");
            if (sourceRepositories.Any(x => x == null)) throw new ArgumentException("Some repositories are null.", "sourceRepositories");

            var resultIterator = UpdateProjectPackageCore(sourceRepositories, projectRepository, projectDescriptor, x => true);
            if ((options & PackageUpdateOptions.Hooks) == PackageUpdateOptions.Hooks)
                resultIterator = WrapWithHooks(resultIterator, projectDescriptor, projectRepository, "project");
            return new PackageUpdateResultIterator(resultIterator);
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

            var resultIterator = UpdateProjectPackageCore(sourceRepositories, projectRepository, projectDescriptor, x => x.EqualsNoCase(packageName));
            if ((options & PackageUpdateOptions.Hooks) == PackageUpdateOptions.Hooks)
                resultIterator = WrapWithHooks(resultIterator, projectDescriptor, projectRepository, "project");
            return new PackageUpdateResultIterator(resultIterator);
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
            return from repo in destinationRepositories
                   let anchorage = repo.Feature<ISupportAnchoring>()
                   where anchorage != null
                   from successfulPackage in resolvedPackages.SuccessfulPackages
                   where successfulPackage.IsAnchored
                   let packageInstances = (from packageInstance in successfulPackage.Packages
                                           where packageInstance != null &&
                                                 packageInstance.Source.Token == repo.Token
                                           select packageInstance)
                   from anchorResult in anchorage.AnchorPackages(packageInstances)
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
                                                           orderby versionedPackage.SemanticVersion descending
                                                           select versionedPackage.SemanticVersion
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
                           let compatiblePackage = packages.FirstOrDefault(x => x.Source.Token == repo.Token)
                           where compatiblePackage != null
                           select compatiblePackage
                   )
                   .First();
        }

        static IPackageInfo GetExistingPackage(IPackageRepository destinationRepository, ResolvedPackage foundPackage, Func<SemanticVersion, bool> versionSelector)
        {
            return destinationRepository.PackagesByName.Contains(foundPackage.Identifier.Name)
                           ? destinationRepository.PackagesByName[foundPackage.Identifier.Name]
                                     .Where(x => foundPackage.Identifier.Version != null && versionSelector(x.SemanticVersion))
                                     .OrderByDescending(x => x.SemanticVersion)
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
                                          ? repository.PackagesByName[packageToRemove.Name].Select(x => x.SemanticVersion)
                                                    .OrderByDescending(_ => _)
                                                    .FirstOrDefault()
                                          : packageToRemove.ExactVersion;
            var packagesToKeep = from package in repository.PackagesByName.SelectMany(_ => _)
                                 let matchesName = package.Name.EqualsNoCase(packageToRemove.Name)
                                 let matchesVersion = versionToRemove == null ? true : package.SemanticVersion == versionToRemove
                                 where !(matchesName && matchesVersion)
                                 select package;
            return ((ISupportCleaning)repository).Clean(packagesToKeep).Cast<PackageOperationResult>();
        }

        static PackageDependency ToDependency(PackageRequest packageToAdd, PackageAddOptions options)
        {
            return new PackageDependencyBuilder(packageToAdd.Name)
                    .SetVersionVertices(ToVersionVertices(packageToAdd))
                    .Anchored((options & PackageAddOptions.Anchor) == PackageAddOptions.Anchor)
                    .Content((options & PackageAddOptions.Content) == PackageAddOptions.Content)
                    .Edge((options & PackageAddOptions.Edge) == PackageAddOptions.Edge);
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
            if (projectDescriptor.Dependencies.Where(x => x.Name.EqualsNoCase(packageToAdd.Name)).Any())
            {
                yield return new PackageDependencyAlreadyExists(packageToAdd.Name);
                yield break;
            }
            
            var dependency = ToDependency(packageToAdd, options);
            projectDescriptor.Dependencies.Add(dependency);
            yield return new PackageDependencyAddedResult(dependency);

            foreach (var m in CopyPackageCore(sourceRepositories, new[] { projectRepository }, projectDescriptor, x => x.EqualsNoCase(packageToAdd.Name)))
                yield return m;
        }

        IEnumerable<PackageOperationResult> AddSystemPackageCore(IEnumerable<IPackageRepository> sourceRepositories,
                                                                 IPackageRepository systemRepository,
                                                                 PackageRequest packageToAdd,
                                                                 PackageAddOptions options)
        {
            return  CopyPackageCore(sourceRepositories, new[] { systemRepository }, ToDescriptor(packageToAdd, options), x => packageToAdd.Name.EqualsNoCase(x));
        }

        IEnumerable<PackageOperationResult> CleanProjectPackagesCore(IEnumerable<IPackageDescriptor> projectDescriptors, IPackageRepository projectRepository, Func<string, bool> packageName)
        {
            var resolvedPackages = projectDescriptors.Select(projectDescriptor=>new{projectDescriptor, result = _resolver.TryResolveDependencies(projectDescriptor, new[] { projectRepository })});

            var failing = resolvedPackages.FirstOrDefault(x=>x.result.SuccessfulPackages.Any() == false);
            if (failing != null)
            {
                yield return new PackageCleanCannotDo(failing.projectDescriptor);
                yield break;
            }
            var projectPackagesInUse = from successfulPackageStack in resolvedPackages.SelectMany(x=>x.result.SuccessfulPackages)
                                       from package in successfulPackageStack.Packages
                                       where packageName(package.Identifier.Name)
                                       select package;

            var otherPackages = from packagesByName in projectRepository.PackagesByName
                                where !packageName(packagesByName.Key)
                                from package in packagesByName
                                select package;
            var packagesInUse = projectPackagesInUse.Concat(otherPackages).ToList();


            foreach (var cleanedPackage in projectRepository.Feature<ISupportCleaning>().Clean(packagesInUse))
                yield return cleanedPackage;
        }

        IEnumerable<PackageOperationResult> CleanSystemPackagesCore(IPackageRepository systemRepository, Func<string, bool> packageNameSelector)
        {
            var selectedPackages = from packageByName in systemRepository.PackagesByName
                               where packageNameSelector(packageByName.Key)
                               select packageByName.OrderByDescending(x => x.SemanticVersion).First();

            var untouchedVersions = systemRepository.PackagesByName.Where(x => !packageNameSelector(x.Key)).SelectMany(x => x);

            foreach (var clean in systemRepository.Feature<ISupportCleaning>().Clean(selectedPackages.Concat(untouchedVersions)))
                yield return clean;
        }

        IEnumerable<PackageOperationResult> CopyPackageCore(IEnumerable<IPackageRepository> sourceRepositories,
                                                            IEnumerable<IPackageRepository> destinationRepositories,
                                                            IPackageDescriptor descriptor,
                                                            Func<string, bool> nameSelector)
        {
            var updateDescriptor = new PackageDescriptor(descriptor);

            var resolvedPackages = _resolver.TryResolveDependencies(
                    updateDescriptor,
                    sourceRepositories);
            
            if (!resolvedPackages.IsSuccess)
            {
                foreach (var packageResolution in ReturnError(resolvedPackages))
                    yield return packageResolution;
                yield break;
            }

            var rootPackages = resolvedPackages.SuccessfulPackages.Where(_ => nameSelector(_.Identifier.Name));
            var affectedPackageNames = new List<string>();
            new PackageGraphVisitor(resolvedPackages.SuccessfulPackages.Select(x => x.Packages.First()))
                .VisitFrom(rootPackages.Select(_ => new PackageDependency(_.Identifier.Name)),
                           (from, dep, to) =>
                           {
                               affectedPackageNames.Add(to.Name);
                               return true;
                           });
            var packagesForGacDetection = GetSelectedPackages(resolvedPackages);

            foreach (var conflict in from errors in _exporter.InGac(packagesForGacDetection)
                                     select new PackageGacConflictResult(errors.Key, errors))
                yield return conflict;

            var packagesToCopy = resolvedPackages.SuccessfulPackages.Where(x=>affectedPackageNames.ContainsNoCase(x.Identifier.Name));
            foreach (var m in CopyPackagesToRepositories(sourceRepositories, packagesToCopy, destinationRepositories))
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

        static IEnumerable<IPackageInfo> GetSelectedPackages(DependencyResolutionResult resolvedPackages)
        {
            return resolvedPackages.SuccessfulPackages.Select(x => x.Packages.First());
        }

        IEnumerable<PackageOperationResult> CopyPackagesToRepositories(IEnumerable<IPackageRepository> sourceRepositories,
                                                                       IEnumerable<ResolvedPackage> resolvedPackages,
                                                                       IEnumerable<IPackageRepository> destinationRepositories)
        {
            var publishingRepos = destinationRepositories.Select(x=>new{repo=x,pub=x.Feature<ISupportPublishing>()}).NotNull().ToList();
            foreach (var destinationRepository in publishingRepos)
            {
                using (var publisher = destinationRepository.pub.Publisher())
                {
                    foreach (var foundPackage in resolvedPackages)
                    {
                        if (foundPackage == null) throw new InvalidOperationException("A null package was selected in the package resolution phase. Something's gone badly wrong.");
                        var package = foundPackage;

                        var existingUpToDateVersion = GetExistingPackage(destinationRepository.repo, package, x => x == package.Identifier.Version);
                        if (existingUpToDateVersion == null)
                        {
                            var sourcePackage = GetBestSourcePackage(sourceRepositories, package.Packages);

                            _deployer.DeployDependency(sourcePackage, publisher);
                            var existingVersion = GetExistingPackage(destinationRepository.repo, package, x => x != package.Identifier.Version);

                            yield return existingVersion == null
                                                 ? new PackageAddedResult(sourcePackage, destinationRepository.repo)
                                                 : new PackageUpdatedResult(existingVersion, sourcePackage, destinationRepository.repo);
                        }
                        else
                        {
                            yield return new PackageUpToDateResult(existingUpToDateVersion, destinationRepository.repo);
                        }
                    }
                }
            }
        }

        IEnumerable<PackageOperationResult> RemoveFromDescriptor(PackageRequest packageToRemove,
                                                                 IPackageDescriptor packageDescriptor,
                                                                 IPackageRepository projectRepository,
                                                                 PackageRemoveOptions options)
        {
            var dependency = packageDescriptor.Dependencies.FirstOrDefault(x => x.Name.EqualsNoCase(packageToRemove.Name));
            if (dependency == null)
            {
                yield return new PackageDescriptorUpdateResult(PackageDescriptorDependencyUpdate.DependencyNotFound);
                yield break;
            }
            packageDescriptor.Dependencies.Remove(dependency);
            yield return new PackageDependencyRemovedResult(dependency);

        }

        IEnumerable<PackageOperationResult> RemoveProjectPackageCore(PackageRequest packageToRemove,
                                                                     IPackageDescriptor packageDescriptor,
                                                                     IPackageRepository projectRepository,
                                                                     PackageRemoveOptions options)
        {
            foreach (var m in (packageToRemove.ExactVersion == null && !packageToRemove.LastVersion)
                           ? RemoveFromDescriptor(packageToRemove, packageDescriptor, projectRepository, options)
                           : RemovePackageFilesFromProjectRepo(packageToRemove, projectRepository))
                yield return m;
        }

        IEnumerable<PackageOperationResult> 
            RemoveSystemPackageCore(PackageRequest packageToRemove, IPackageRepository systemRepository)
        {
            return RemovePackageFromRepository(packageToRemove, systemRepository);
        }

        IEnumerable<PackageOperationResult> ReturnError(DependencyResolutionResult resolvedPackages)
        {
            return resolvedPackages.DiscardedPackages.Select(PackageConflict)
                    .Concat(resolvedPackages.MissingPackages.Select(PackageMissing));
        }

        IEnumerable<PackageOperationResult> UpdateProjectPackageCore(IEnumerable<IPackageRepository> sourceRepositories,
                                                                     IPackageRepository projectRepository,
                                                                     IPackageDescriptor projectDescriptor,
                                                                     Func<string, bool> nameSelector)
        {

            foreach (var m in CopyPackageCore(sourceRepositories, new[] { projectRepository }, projectDescriptor, nameSelector))
                yield return m;

            var resolved = _resolver.TryResolveDependencies(projectDescriptor, sourceRepositories);
            if (resolved.IsSuccess)
            {
                foreach (var missing in from package in resolved.SuccessfulPackages
                                        let name = package.Identifier.Name
                                        where nameSelector(name) &&
                                        sourceRepositories.Where(_=>_ != projectRepository).Any(repo => repo.PackagesByName.Contains(name)) == false
                                        select name)
                    yield return new PackageOnlyInCurrentRepository(missing, projectRepository);
            }
        }

        IEnumerable<PackageOperationResult> UpdateSystemPackageCore(IEnumerable<IPackageRepository> sourceRepositories, IPackageRepository systemRepository, Func<string, bool> packageNameSelector)
        {
            return CreateDescriptorForEachSystemPackage(systemRepository, packageNameSelector)
                    .SelectMany(x => CopyPackageCore(sourceRepositories, new[] { systemRepository }, x, name => true));
        }

        class UpdatePackageVertex : VersionVertex
        {
            public UpdatePackageVertex(SemanticVersion existingVersion)
                : base(existingVersion)
            {
            }

            public override bool IsCompatibleWith(SemanticVersion version)
            {
                return Version <= version;
            }
            public override string ToString()
            {
                return string.Empty;
            }
        }
    }
}
