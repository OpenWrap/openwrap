using OpenWrap.Services;

namespace OpenWrap
{
    public static class InternalServices
    {
        public static void Initialize()
        {
            WrapServices.TryRegisterService<IWrapDescriptorMonitoringService>(() => new WrapDescriptorMonitor());
        }
    }
}