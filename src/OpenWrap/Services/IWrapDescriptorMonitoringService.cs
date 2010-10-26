using OpenWrap.Build;
using OpenFileSystem.IO;
using OpenWrap.Repositories;

namespace OpenWrap.Services
{
    public interface IWrapDescriptorMonitoringService : IService
    {
        void ProcessWrapDescriptor(IFile wrapPath, IPackageRepository packageRepository, IPackageAssembliesListener listener);
    }
}