using OpenWrap.PackageModel;

namespace OpenWrap.PackageManagement.DependencyResolvers
{
    public class DependencyNode : Node
    {
        public DependencyNode(PackageDependency dependency)
        {
            Dependency = dependency;
        }

        public PackageDependency Dependency { get; private set; }

        public override string ToString()
        {
            return Dependency.ToString();
        }
    }
}