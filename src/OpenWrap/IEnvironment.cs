using OpenRasta.Wrap.Build.Services;
using OpenRasta.Wrap.Sources;

namespace OpenWrap
{
    public interface IEnvironment : IService
    {
        IWrapRepository Repository { get; }
        string WrapDescriptorPath { get; }
    }
}