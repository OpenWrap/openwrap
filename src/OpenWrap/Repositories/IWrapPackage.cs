using System.Diagnostics;
using OpenRasta.Wrap.Dependencies;
using OpenRasta.Wrap.Resources;

namespace OpenRasta.Wrap.Sources
{
    public interface IWrapPackage : IWrapPackageInfo
    {
        IWrapExport GetExport(string exportName, WrapRuntimeEnvironment environment);
    }
}