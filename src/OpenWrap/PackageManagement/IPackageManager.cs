using System;
using System.Collections.Generic;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;

namespace OpenWrap.PackageManagement
{
    public interface IPackageManager
    {
        IPackageUpdateResult UpdateSystemPackages(IEnumerable<IPackageRepository> sourceRepositories,
                                                  IPackageRepository systemRepository,
                                                  PackageUpdateOptions options = PackageUpdateOptions.Default);
        IPackageUpdateResult UpdateSystemPackages(IEnumerable<IPackageRepository> sourceRepositories, IPackageRepository systemRepository, string packageName,
                                                  PackageUpdateOptions options = PackageUpdateOptions.Default);
        IPackageUpdateResult UpdateProjectPackages(IEnumerable<IPackageRepository> sourceRepositories, IPackageRepository projectRepository, PackageDescriptor projectDescriptor,
                                                  PackageUpdateOptions options = PackageUpdateOptions.Default);

        IPackageUpdateResult UpdateProjectPackages(IEnumerable<IPackageRepository> sourceRepositories,
                                                   IPackageRepository projectRepository,
                                                   PackageDescriptor projectDescriptor,
                                                   string packageName,
                                                  PackageUpdateOptions options = PackageUpdateOptions.Default);

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

        IPackageCleanResult CleanProjectPackages(PackageDescriptor projectDescriptor, IPackageRepository projectRepository, PackageCleanOptions options = PackageCleanOptions.Default);
        IPackageCleanResult CleanProjectPackages(PackageDescriptor projectDescriptor, IPackageRepository projectRepository, string packageName, PackageCleanOptions options = PackageCleanOptions.Default);
        IPackageCleanResult CleanSystemPackages(IPackageRepository systemRepository, PackageCleanOptions options = PackageCleanOptions.Default);
        IPackageCleanResult CleanSystemPackages(IPackageRepository systemRepository, string packageName, PackageCleanOptions options = PackageCleanOptions.Default);

        IPackageListResult ListPackages(IEnumerable<IPackageRepository> repositories, string query = null, PackageListOptions options = PackageListOptions.Default);
    }
    [Flags]
    public enum PackageUpdateOptions
    {
        Recurse = 1,
        Default = Recurse
    }
    [Flags]
    public enum PackageCleanOptions
    {
        Default
    }
    [Flags]
    public enum PackageListOptions
    {
        Default
    }
}