using OpenWrap.Build;
using OpenWrap.IO;
using OpenWrap.Repositories;

namespace OpenWrap.Services
{
    public interface IWrapDescriptorMonitoringService : IService
    {
        void ProcessWrapDescriptor(IFile wrapPath, IPackageRepository packageRepository, IWrapAssemblyClient client);
    }
}