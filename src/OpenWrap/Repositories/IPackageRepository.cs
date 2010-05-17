using System.Linq;
using OpenRasta.Wrap.Dependencies;

namespace OpenRasta.Wrap.Sources
{
    public interface IPackageRepository
    {
        ILookup<string, IPackageInfo> PackagesByName { get; }
        IPackageInfo Find(WrapDependency dependency);
    }
}