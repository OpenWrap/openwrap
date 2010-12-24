using OpenWrap.PackageModel;

namespace OpenWrap.PackageManagement.DependencyResolvers
{
    public class PackageNode : Node
    {
        public PackageNode(PackageIdentifier identifier)
        {
            Identifier = identifier;
        }

        public PackageIdentifier Identifier { get; private set; }

        public override string ToString()
        {
            return Identifier.ToString();
        }
    }
}