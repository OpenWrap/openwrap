using System.IO;
using System.Linq;
using OpenWrap.Dependencies;

namespace OpenWrap.Repositories
{
    public interface IPackageRepository
    {
        ILookup<string, IPackageInfo> PackagesByName { get; }
        IPackageInfo Find(WrapDependency dependency);
        IPackageInfo Publish(string packageFileName, Stream packageStream);
    }
}
