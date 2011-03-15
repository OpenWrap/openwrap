using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using OpenWrap.Collections;
using OpenWrap.PackageManagement.DependencyResolvers;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;

namespace OpenWrap.PackageManagement
{
    public static class PackageResultExtensions
    {
        public static IEnumerable<PackageOperationResult> Hooks(this IEnumerable<PackageOperationResult> results)
        {
            bool success = true;
            foreach (var result in results)
            {
                if (result.Success == false)
                    success = false;
                yield return result;
            }
        }
    }

    public delegate IEnumerable<object> PackageUpdated(string repository, string name, Version fromVersion, Version toVersion, IEnumerable<IPackageInfo> packages);

    public delegate IEnumerable<object> PackageChanged(string repository, string name, Version version, IEnumerable<IPackageInfo> packages);


    public class HooksStore
    {
        public event PackageChanged PackageAdded;
        public event PackageChanged PackageRemoved;
        public event PackageUpdated PackageUpdated;

        public HooksStore()
        {   
        }
        public IEnumerable<object> Installed(string repository, string packageName, Version version, IEnumerable<IPackageInfo> packages)
        {
            var hooks = PackageAdded;
            if (hooks != null)
                return hooks.GetInvocationList().SelectMany(x => (IEnumerable<object>)x.DynamicInvoke(repository, packageName, version, packages));
                //return hooks.I((repository, packageName, version, packages);
            return Enumerable.Empty<object>();
        }
        public IEnumerable<object> Updated(string repository, string packageName, Version fromVersion, Version toVersion, IEnumerable<IPackageInfo> packages)
        {
            var hooks = PackageUpdated;
            if (hooks != null)
                return hooks.GetInvocationList().SelectMany(x => (IEnumerable<object>)x.DynamicInvoke(repository, packageName, fromVersion, toVersion, packages));

            return Enumerable.Empty<object>();
        }
        public IEnumerable<object> Removed(string repository, string packageName, Version version, IEnumerable<IPackageInfo> packages)
        {
            var hooks = PackageRemoved;
            if (hooks != null)
                return hooks.GetInvocationList().SelectMany(x => (IEnumerable<object>)x.DynamicInvoke(repository, packageName, version, packages));

            return Enumerable.Empty<object>();
        }
    }
    public class HookedPackageOperationResults : IEnumerable<PackageOperationResult>
    {
        readonly string _repository;
        readonly IEnumerable<PackageOperationResult> _results;
        readonly HooksStore _hooks;
        readonly Func<IEnumerable<IPackageInfo>> _before;
        readonly Func<IEnumerable<IPackageInfo>> _after;

        public HookedPackageOperationResults(string repository, IEnumerable<PackageOperationResult> results, HooksStore hooks, Func<IEnumerable<IPackageInfo>> before, Func<IEnumerable<IPackageInfo>> after)
        {
            _repository = repository;
            _results = results;
            _hooks = hooks;
            _before = before;
            _after = after;
        }

        IEnumerator<PackageOperationResult> IEnumerable<PackageOperationResult>.GetEnumerator()
        {
            var resolvedBefore = _before();

            bool? success = null;
            foreach (var result in _results)
            {
                success = success ?? true;
                if (!result.Success)
                    success = false;
                yield return result;
            }
            if (success == true)
            {
                var resolvedAfter = _after();
                // TODO: process removes, then process adds and updates based on a dependency tree
                foreach (var output in GetRemoved(resolvedBefore, resolvedAfter, _hooks).Concat(GetAdded(resolvedBefore, resolvedAfter, _hooks)).Concat(GetUpdated(resolvedBefore, resolvedAfter, _hooks)))
                    yield return new PackageHookResult(output);
            }
        }
        IEnumerable<object> GetUpdated(IEnumerable<IPackageInfo> before, IEnumerable<IPackageInfo> after, HooksStore hooks)
        {
            var afterByName = after.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
            return from oldPackage in before
                   where afterByName.ContainsKey(oldPackage.Name)
                   let newPackage = afterByName[oldPackage.Name]
                   where newPackage.Version != oldPackage.Version
                   from output in hooks.Updated(_repository, newPackage.Name, oldPackage.Version, newPackage.Version, after)
                   select output;
        }

        IEnumerable<object> GetAdded(IEnumerable<IPackageInfo> before, IEnumerable<IPackageInfo> after, HooksStore hooks)
        {
            var beforeByName = before.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
            return from newPackage in after
                   where beforeByName.ContainsKey(newPackage.Name) == false
                   from output in hooks.Installed(_repository, newPackage.Name, newPackage.Version, after)
                   select output;
        }
        IEnumerable<object> GetRemoved(IEnumerable<IPackageInfo> before, IEnumerable<IPackageInfo> after, HooksStore hooks)
        {
            var afterByName = after.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
            return from oldPackage in before
                   where afterByName.ContainsKey(oldPackage.Name) == false
                   from output in hooks.Removed(_repository, oldPackage.Name, oldPackage.Version, before)
                   select output;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<PackageOperationResult>)this).GetEnumerator();
        }
    }
    public static class PackageManagerExtensions
    {
        public static IDisposable Monitor(this IPackageManager manager, PackageChanged add  = null, PackageChanged remove = null, PackageUpdated update = null)
        {
            if (add != null) manager.PackageAdded += add;
            if (remove != null) manager.PackageRemoved += remove;
            if (update != null) manager.PackageUpdated += update;
            return new ActionOnDispose(() =>
            {
                if (add != null) manager.PackageAdded -= add;
                if (remove != null) manager.PackageRemoved -= remove;
                if (update != null) manager.PackageUpdated -= update;
            });
        }
    }
    public class DefaultPackageManager : IPackageManager
    {
        readonly IPackageDeployer _deployer;

        readonly IPackageResolver _resolver;
        readonly HooksStore _hooks = new HooksStore();


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

        public DefaultPackageManager(IPackageDeployer deployer, IPackageResolver resolver)
        {
            _deployer = deployer;
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

        public IPackageCleanResult CleanProjectPackages(IPackageDescriptor packages, IPackageRepository projectRepository, PackageCleanOptions options = PackageCleanOptions.Default)
        {
            if (packages == null) throw new ArgumentNullException("packages");
            if (projectRepository == null) throw new ArgumentNullException("Repository");

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

        public IPackageRemoveResult RemoveProjectPackage(PackageRequest packageToRemove,
                                                         IPackageDescriptor packageDescriptor,
                                                         IPackageRepository projectRepository,
                                                         PackageRemoveOptions options = PackageRemoveOptions.Default)
        {
            if (packageToRemove == null) throw new ArgumentNullException("packageToRemove");
            if (packageDescriptor == null) throw new ArgumentNullException("packageDescriptor");
            if (projectRepository == null) throw new ArgumentNullException("projectRepository");

            return new PackageRemoveResultIterator(RemoveProjectPackageCore(packageToRemove, packageDescriptor, projectRepository, options));
        }

        public IPackageRemoveResult RemoveSystemPackage(PackageRequest packageToRemove, IPackageRepository systemRepository, PackageRemoveOptions options = PackageRemoveOptions.Default)
        {
            if (packageToRemove == null) throw new ArgumentNullException("packageToRemove");
            if (systemRepository == null) throw new ArgumentNullException("systemRepository");
            return new PackageRemoveResultIterator(RemoveSystemPackageCore(packageToRemove, systemRepository));
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

            return new PackageUpdateResultIterator(UpdateProjectPackageCore(sourceRepositories, projectRepository, projectDescriptor, x => true));
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

            return new PackageUpdateResultIterator(UpdateProjectPackageCore(sourceRepositories, projectRepository, projectDescriptor, x => x.EqualsNoCase(packageName)));
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

        IEnumerable<PackageOperationResult> AddSystemPackageCore(IEnumerable<IPackageRepository> sourceRepositories,
                                                                 IPackageRepository systemRepository,
                                                                 PackageRequest packageToAdd,
                                                                 PackageAddOptions options)
        {
            return  CopyPackageCore(sourceRepositories, new[] { systemRepository }, ToDescriptor(packageToAdd, options), x => true);
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
            var selectedPackages = from packageByName in systemRepository.PackagesByName
                               where packageNameSelector(packageByName.Key)
                               select packageByName.OrderByDescending(x => x.Version).First();

            var untouchedVersions = systemRepository.PackagesByName.Where(x => !packageNameSelector(x.Key)).SelectMany(x => x);

            foreach (var clean in systemRepository.Clean(selectedPackages.Concat(untouchedVersions)))
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

            var packagesForGacDetection = GetSelectedPackages(resolvedPackages);

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

        static IEnumerable<IPackageInfo> GetSelectedPackages(DependencyResolutionResult resolvedPackages)
        {
            return resolvedPackages.SuccessfulPackages.Select(x => x.Packages.First());
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
                        if (foundPackage == null) throw new InvalidOperationException("A null package was selected in the package resolution phase. Something's gone badly wrong.");
                        var package = foundPackage;

                        var existingUpToDateVersion = GetExistingPackage(destinationRepository, package, x => x == package.Identifier.Version);
                        if (existingUpToDateVersion == null)
                        {
                            var sourcePackage = GetBestSourcePackage(sourceRepositories, package.Packages);

                            _deployer.DeployDependency(sourcePackage, publisher);
                            var existingVersion = GetExistingPackage(destinationRepository, package, x => x != package.Identifier.Version);

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
            if ((options & PackageRemoveOptions.Clean) == PackageRemoveOptions.Clean)
                foreach (var cleaned in CleanProjectPackages(packageDescriptor, projectRepository, packageToRemove.Name))
                    yield return cleaned;
        }

        IEnumerable<PackageOperationResult> RemoveProjectPackageCore(PackageRequest packageToRemove,
                                                                     IPackageDescriptor packageDescriptor,
                                                                     IPackageRepository projectRepository,
                                                                     PackageRemoveOptions options)
        {
            return (packageToRemove.ExactVersion == null && !packageToRemove.LastVersion)
                           ? RemoveFromDescriptor(packageToRemove, packageDescriptor, projectRepository, options)
                           : RemovePackageFilesFromProjectRepo(packageToRemove, projectRepository);
        }

        IEnumerable<PackageOperationResult> RemoveSystemPackageCore(PackageRequest packageToRemove, IPackageRepository systemRepository)
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
            return CopyPackageCore(sourceRepositories, new[] { projectRepository }, projectDescriptor, nameSelector);
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