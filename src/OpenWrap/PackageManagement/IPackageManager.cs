using System.Collections.Generic;
using System.Linq;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;

namespace OpenWrap.PackageManagement
{
    public interface IPackageManager
    {
        IPackageUpdateResult UpdateSystemPackages(IEnumerable<IPackageRepository> sourceRepositories,
                                                  IPackageRepository systemRepository,
                                                  PackageUpdateOptions options = PackageUpdateOptions.Default);

        IPackageUpdateResult UpdateSystemPackages(IEnumerable<IPackageRepository> sourceRepositories,
                                                  IPackageRepository systemRepository,
                                                  string packageName,
                                                  PackageUpdateOptions options = PackageUpdateOptions.Default);

        IPackageUpdateResult UpdateProjectPackages(IEnumerable<IPackageRepository> sourceRepositories,
                                                   IPackageRepository projectRepository,
                                                   IPackageDescriptor projectDescriptor,
                                                   PackageUpdateOptions options = PackageUpdateOptions.Recurse);

        IPackageUpdateResult UpdateProjectPackages(IEnumerable<IPackageRepository> sourceRepositories,
                                                   IPackageRepository projectRepository,
                                                   IPackageDescriptor projectDescriptor,
                                                   string packageName,
                                                   PackageUpdateOptions options = PackageUpdateOptions.Default);

        IPackageAddResult AddProjectPackage(PackageRequest packageToAdd,
                                            IEnumerable<IPackageRepository> sourceRepositories,
                                            IPackageDescriptor projectDescriptor,
                                            IPackageRepository projectRepository,
                                            PackageAddOptions options = PackageAddOptions.Default);

        IPackageAddResult AddSystemPackage(PackageRequest packageToAdd,
                                           IEnumerable<IPackageRepository> sourceRepositories,
                                           IPackageRepository systemRepository,
                                           PackageAddOptions options = PackageAddOptions.Default);

        IPackageRemoveResult RemoveProjectPackage(PackageRequest packageToRemove,
                                                  IPackageDescriptor packageDescriptor,
                                                  IPackageRepository projectRepository,
                                                  PackageRemoveOptions optiosn = PackageRemoveOptions.Default);

        IPackageRemoveResult RemoveSystemPackage(PackageRequest packageToRemove, IPackageRepository systemRepository, PackageRemoveOptions options = PackageRemoveOptions.Default);

        IPackageCleanResult CleanProjectPackages(IEnumerable<IPackageDescriptor> projectDescriptors, IPackageRepository projectRepository, PackageCleanOptions options = PackageCleanOptions.Default);

        IPackageCleanResult CleanProjectPackages(IEnumerable<IPackageDescriptor> projectDescriptors,
                                                 IPackageRepository projectRepository,
                                                 string packageName,
                                                 PackageCleanOptions options = PackageCleanOptions.Default);

        IPackageCleanResult CleanSystemPackages(IPackageRepository systemRepository, PackageCleanOptions options = PackageCleanOptions.Default);
        IPackageCleanResult CleanSystemPackages(IPackageRepository systemRepository, string packageName, PackageCleanOptions options = PackageCleanOptions.Default);

        IPackageListResult ListPackages(IEnumerable<IPackageRepository> repositories, string query = null, PackageListOptions options = PackageListOptions.Default, IEnumerable<IPackageInfo> currentPackages = null);
        
        event PackageUpdated PackageUpdated;
        event PackageChanged PackageAdded;
        event PackageChanged PackageRemoved;
    }
}