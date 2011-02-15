using OpenWrap.PackageManagement.Monitoring;

namespace OpenWrap.Services
{
    public static class InternalServices
    {
        public static void Initialize()
        {
            ServiceLocator.TryRegisterService<IPackageDescriptorMonitor>(() => new PackageDescriptorMonitor());
        }
    }
}