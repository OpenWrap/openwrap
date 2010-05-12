using System.Linq;
using OpenRasta.Wrap.Dependencies;

namespace OpenRasta.Wrap.Sources
{
    public interface IWrapRepository
    {
        ILookup<string, IWrapPackageInfo> PackagesByName { get; }
        IWrapPackageInfo Find(WrapDependency dependency);
    }
}