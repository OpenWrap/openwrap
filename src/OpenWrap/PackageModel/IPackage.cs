using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenFileSystem.IO;
using OpenWrap.PackageManagement;
using OpenWrap.PackageManagement.Exporters;
using OpenWrap.Runtime;

namespace OpenWrap.PackageModel
{
    /// <summary>
    ///   Represents a wrap package.
    /// </summary>
    public interface IPackage : IPackageInfo
    {
        IEnumerable<IGrouping<string, Exports.IFile>> Content { get; }
        IPackageDescriptor Descriptor { get; }
        Stream OpenStream();
    }
}