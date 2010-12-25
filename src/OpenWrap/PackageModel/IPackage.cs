using System.IO;
using OpenWrap.PackageManagement;
using OpenWrap.Runtime;

namespace OpenWrap.PackageModel
{
    /// <summary>
    ///   Represents a wrap package.
    /// </summary>
    public interface IPackage : IPackageInfo
    {
        PackageDescriptor Descriptor { get; }
        IExport GetExport(string exportName, ExecutionEnvironment environment);
        Stream OpenStream();
    }
}