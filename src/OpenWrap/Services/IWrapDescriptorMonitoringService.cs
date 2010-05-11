using OpenRasta.Wrap.Sources;

namespace OpenRasta.Wrap.Build.Services
{
    public interface IWrapDescriptorMonitoringService : IService
    {
        void ProcessWrapDescriptor(string wrapPath, IWrapRepository wrapsDirectory, IWrapAssemblyClient client);
    }
}