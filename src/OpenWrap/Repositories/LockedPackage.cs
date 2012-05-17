namespace OpenWrap.Repositories
{
    public class LockedPackage
    {
        public LockedPackage()
        {
        }
        public LockedPackage(string name, SemanticVersion version)
        {
            Name = name;
            Version = version;
        }

        public string Name { get; set; }
        public SemanticVersion Version { get; set; }
   
    }
}