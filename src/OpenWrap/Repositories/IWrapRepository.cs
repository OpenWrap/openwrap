using System.Linq;
using OpenRasta.Wrap.Dependencies;

namespace OpenRasta.Wrap.Sources
{
    public interface IWrapRepository
    {
        ILookup<string, IWrapPackage> PackagesByName { get; }
        IWrapPackage Find(WrapDependency dependency);
    }
}