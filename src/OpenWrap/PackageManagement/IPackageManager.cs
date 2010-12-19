using System;
using System.Collections;
using System.Collections.Generic;
using OpenWrap.Dependencies;
using OpenWrap.Repositories;

namespace OpenWrap.PackageManagement
{
    public interface IPackageManager
    {
        IPackageUpdateResult UpdateSystemPackages(IEnumerable<IPackageRepository> sourceRepositories, IPackageRepository systemRepository);
        IPackageUpdateResult UpdateSystemPackages(IEnumerable<IPackageRepository> sourceRepositories, IPackageRepository systemRepository, string packageName);
        IPackageUpdateResult UpdateProjectPackages(IEnumerable<IPackageRepository> sourceRepositories, IPackageRepository projectRepository, PackageDescriptor projectDescriptor);

        IPackageUpdateResult UpdateProjectPackages(IEnumerable<IPackageRepository> sourceRepositories,
                                                                  IPackageRepository projectRepository,
                                                                  PackageDescriptor projectDescriptor,
                                                                  string packageName);

        IPackageAddResult AddProjectPackage(PackageRequest packageToAdd,
                                                              IEnumerable<IPackageRepository> sourceRepositories,
                                                              PackageDescriptor projectDescriptor,
                                                              IPackageRepository projectRepository,
                                                              PackageAddOptions options = PackageAddOptions.Default);

        IPackageAddResult AddSystemPackage(PackageRequest packageToAdd,
                                                             IEnumerable<IPackageRepository> sourceRepositories,
                                                             IPackageRepository systemRepository,
                                                             PackageAddOptions options = PackageAddOptions.Default);

        IPackageRemoveResult RemoveProjectPackage(PackageRequest packageToRemove,
                                                                 PackageDescriptor packageDescriptor,
                                                                 IPackageRepository projectRepository,
                                                                 PackageRemoveOptions optiosn = PackageRemoveOptions.Default);

        IPackageRemoveResult RemoveSystemPackage(PackageRequest packageToRemove, IPackageRepository systemRepository, PackageRemoveOptions options = PackageRemoveOptions.Default);

        IPackageCleanResult CleanProjectPackages(PackageDescriptor projectDescriptor, IPackageRepository projectRepository);
        IPackageCleanResult CleanProjectPackages(PackageDescriptor projectDescriptor, IPackageRepository projectRepository, string packageName);
        IPackageCleanResult CleanSystemPackages(IPackageRepository systemRepository);
        IPackageCleanResult CleanSystemPackages(IPackageRepository systemRepository, string packageName);

        IPackageListResult ListPackages(IEnumerable<IPackageRepository> repositories, string query = null);
    }

    public interface IPackageUpdateResult : IEnumerable<PackageOperationResult>
    {
    }

    public interface IPackageAddResult : IEnumerable<PackageOperationResult>
    {
    }

    public interface IPackageRemoveResult : IEnumerable<PackageOperationResult>
    {
    }

    public interface IPackageCleanResult : IEnumerable<PackageOperationResult>
    {
    }

    public interface IPackageListResult : IEnumerable<PackageOperationResult>
    {
    }

    public abstract class AbstractPackageOperation : IEnumerable<PackageOperationResult>
    {
        readonly IEnumerable<PackageOperationResult> _root;

        public AbstractPackageOperation(IEnumerable<PackageOperationResult> root)
        {
            _root = root;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<PackageOperationResult>)this).GetEnumerator();
        }

        IEnumerator<PackageOperationResult> IEnumerable<PackageOperationResult>.GetEnumerator()
        {
            foreach (var value in _root)
                yield return value;
        }
    }
    public class PackageUpdateResultIterator : AbstractPackageOperation, IPackageUpdateResult
    {
        public PackageUpdateResultIterator(IEnumerable<PackageOperationResult> root) : base(root) { }
    }
    public class PackageAddResultIterator : AbstractPackageOperation, IPackageAddResult
    {
        public PackageAddResultIterator(IEnumerable<PackageOperationResult> root) : base(root) { }
    }
    public class PackageListResultIterator : AbstractPackageOperation, IPackageListResult
    {
        public PackageListResultIterator(IEnumerable<PackageOperationResult> root) : base(root) { }
    }
    public class PackageRemoveResultIterator : AbstractPackageOperation, IPackageRemoveResult
    {
        public PackageRemoveResultIterator(IEnumerable<PackageOperationResult> root) : base(root) { }
    }
    public class PackageCleanResultIterator : AbstractPackageOperation, IPackageCleanResult
    {
        public PackageCleanResultIterator(IEnumerable<PackageOperationResult> root) : base(root) { }
    }
}