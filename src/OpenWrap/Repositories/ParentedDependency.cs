using OpenWrap.Dependencies;

namespace OpenWrap.Repositories
{
    public class ParentedDependency
    {
        public ParentedDependency(PackageDependency dependency, ParentedDependency parent)
        {
            Dependency = dependency;
            Parent = parent;
        }

        public PackageDependency Dependency { get; private set; }
        public ParentedDependency Parent { get; private set; }
    }
}