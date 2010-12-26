using OpenWrap.PackageModel;
using OpenWrap.Repositories;

namespace OpenWrap.PackageManagement.Deployers
{
    public class DefaultPackageDeployer : IPackageDeployer
    {
        public void DeployDependency(IPackageInfo resolvedPackage,
                                     IPackagePublisher publisher)
        {
            var source = resolvedPackage.Load();
            using (var packageStream = source.OpenStream())
                publisher.Publish(resolvedPackage.FullName + ".wrap", packageStream);
        }

        public void Initialize()
        {
        }
    }
}