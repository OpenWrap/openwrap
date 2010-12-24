using OpenWrap.PackageModel;
using OpenWrap.Repositories;
using OpenWrap.Services;

namespace OpenWrap.PackageManagement
{
    public interface IPackageDeployer : IService
    {
        void DeployDependency(IPackageInfo resolvedPackage,
                              IPackagePublisher destinationRepository);
    }
}