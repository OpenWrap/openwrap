using System.Diagnostics;
using System.IO;
using OpenWrap.Exports;
using OpenWrap.Dependencies;

namespace OpenWrap.Repositories
{
    /// <summary>
    /// Represents a wrap package.
    /// </summary>
    public interface IPackage : IPackageInfo
    {
        IExport GetExport(string exportName, WrapRuntimeEnvironment environment);
        Stream OpenStream();
    }
}