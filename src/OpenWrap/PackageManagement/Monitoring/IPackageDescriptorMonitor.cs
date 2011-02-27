using OpenFileSystem.IO;
using OpenWrap.Repositories;
using OpenWrap.Services;

namespace OpenWrap.PackageManagement.Monitoring
{
    public interface IPackageDescriptorMonitor : IService
    {
        void RegisterListener(IFile descriptorFile, IPackageRepository projectRepository, IResolvedAssembliesUpdateListener listener);
        void UnregisterListener(IResolvedAssembliesUpdateListener listener);
    }
}