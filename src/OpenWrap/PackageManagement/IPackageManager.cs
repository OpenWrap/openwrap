using System.Collections.Generic;
using OpenWrap.Dependencies;
using OpenWrap.Repositories;

namespace OpenWrap.PackageManagement
{
    public interface IPackageManager
    {
        IEnumerable<PackageOperationResult> UpdateSystemPackages(IEnumerable<IPackageRepository> sourceRepositories, IPackageRepository systemRepository);
        IEnumerable<PackageOperationResult> UpdateSystemPackages(IEnumerable<IPackageRepository> sourceRepositories, IPackageRepository systemRepository, string packageName);
        IEnumerable<PackageOperationResult> UpdateProjectPackages(IEnumerable<IPackageRepository> sourceRepositories, IPackageRepository projectRepository, PackageDescriptor projectDescriptor);

        IEnumerable<PackageOperationResult> UpdateProjectPackages(IEnumerable<IPackageRepository> sourceRepositories,
                                                                  IPackageRepository projectRepository,
                                                                  PackageDescriptor projectDescriptor,
                                                                  string packageName);

        IEnumerable<PackageOperationResult> AddProjectPackage(PackageRequest packageToAdd,
                                                              IEnumerable<IPackageRepository> sourceRepositories,
                                                              PackageDescriptor projectDescriptor,
                                                              IPackageRepository projectRepository,
                                                              PackageAddOptions options = PackageAddOptions.Default);

        IEnumerable<PackageOperationResult> AddSystemPackage(PackageRequest packageToAdd,
                                                             IEnumerable<IPackageRepository> sourceRepositories,
                                                             IPackageRepository systemRepository,
                                                             PackageAddOptions options = PackageAddOptions.Default);

        IEnumerable<PackageOperationResult> RemoveProjectPackage(PackageRequest packageToRemove,
                                                                 PackageDescriptor packageDescriptor,
                                                                 IPackageRepository projectRepository,
                                                                 PackageRemoveOptions optiosn = PackageRemoveOptions.Default);

        IEnumerable<PackageOperationResult> RemoveSystemPackage(PackageRequest packageToRemove, IPackageRepository systemRepository, PackageRemoveOptions options = PackageRemoveOptions.Default);

        IEnumerable<PackageOperationResult> CleanProjectPackages(PackageDescriptor projectDescriptor, IPackageRepository projectRepository);
        IEnumerable<PackageOperationResult> CleanProjectPackages(PackageDescriptor projectDescriptor, IPackageRepository projectRepository, string packageName);
        IEnumerable<PackageOperationResult> CleanSystemPackages(IPackageRepository systemRepository);
        IEnumerable<PackageOperationResult> CleanSystemPackages(IPackageRepository systemRepository, string packageName);
    }
}