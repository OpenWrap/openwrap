using OpenWrap.Dependencies;

namespace OpenWrap.Repositories
{
    public class DependencyNode : Node
    {
        public PackageDependency Dependency { get; private set; }

        public DependencyNode(PackageDependency dependency)
        {
            Dependency = dependency;
        }
        public override string ToString()
        {
            return Dependency.ToString();
        }
    }
}