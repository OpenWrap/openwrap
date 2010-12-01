using OpenWrap.Services;

namespace OpenWrap.Repositories
{
    public interface IPackageDeployer : IService
    {
        void DeployDependency(IPackageInfo resolvedPackage,
                              ISupportPublishing destinationRepository);
    }
}