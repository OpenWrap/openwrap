using OpenWrap.PackageManagement.Monitoring;

namespace OpenWrap.Services
{
    public static class InternalServices
    {
        public static void Initialize()
        {
            Services.TryRegisterService<IPackageDescriptorMonitor>(() => new PackageDescriptorMonitor());
        }
    }
}