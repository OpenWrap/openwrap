using OpenWrap.Repositories;

namespace OpenWrap.Build.Services
{
    public interface IWrapDescriptorMonitoringService : IService
    {
        void ProcessWrapDescriptor(string wrapPath, IPackageRepository packageRepository, IWrapAssemblyClient client);
    }
}