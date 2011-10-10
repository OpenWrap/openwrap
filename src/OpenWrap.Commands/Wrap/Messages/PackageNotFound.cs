namespace OpenWrap.Commands.Wrap
{
    public class PackageNotFound : Error
    {
        public string PackageName { get; set; }

        public PackageNotFound(string packageName)
            : base("Package with name'{0}' was not found.", packageName)
        {
            PackageName = packageName;
        }
    }
}