using OpenWrap.Dependencies;

namespace OpenWrap.Repositories
{
    public class ResolvedDependency
    {
        public WrapDependency Dependency { get; set; }
        public IPackageInfo Package { get; set; }
        public IPackageInfo ParentPackage { get; set; }
    }
}