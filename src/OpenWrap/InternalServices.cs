using OpenWrap.Services;

namespace OpenWrap
{
    public static class InternalServices
    {
        public static void Initialize()
        {
            Services.Services.TryRegisterService<IWrapDescriptorMonitoringService>(() => new PackageDescriptorMonitor());
        }
    }
}