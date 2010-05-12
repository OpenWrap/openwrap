using System.Diagnostics;
using System.IO;
using OpenRasta.Wrap.Dependencies;
using OpenRasta.Wrap.Resources;

namespace OpenRasta.Wrap.Sources
{
    /// <summary>
    /// Represents a wrap package.
    /// </summary>
    public interface IWrapPackage : IWrapPackageInfo
    {
        IWrapExport GetExport(string exportName, WrapRuntimeEnvironment environment);
        void Persist(string folder);
    }
}