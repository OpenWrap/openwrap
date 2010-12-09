namespace OpenWrap.Repositories
{
    public class PackageNode : Node
    {
        public PackageIdentifier Identifier { get; private set; }

        public PackageNode(PackageIdentifier identifier)
        {
            Identifier = identifier;
        }
        public override string ToString()
        {
            return Identifier.ToString();
        }
    }
}