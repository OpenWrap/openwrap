using System.IO;
using OpenWrap.Dependencies;
using OpenWrap.Exports;

namespace OpenWrap.Repositories
{
    /// <summary>
    /// Represents a wrap package.
    /// </summary>
    public interface IPackage : IPackageInfo
    {
        IExport GetExport(string exportName, ExecutionEnvironment environment);
        Stream OpenStream();
    }
}
