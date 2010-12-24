using OpenWrap.PackageManagement.DependencyResolvers;
using OpenWrap.Repositories;

namespace OpenWrap.PackageManagement
{
    public abstract class PackageResolveError : PackageOperationResult
    {
        public PackageResolveError(ResolvedPackage result)
        {
            Package = result;
        }

        public ResolvedPackage Package { get; private set; }
    }
}