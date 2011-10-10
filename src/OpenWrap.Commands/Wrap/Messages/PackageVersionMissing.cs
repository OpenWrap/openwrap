namespace OpenWrap.Commands.Wrap
{
    public class PackageVersionMissing : Error
    {
        public PackageVersionMissing():base("No version was found for this package. Try putting the value in a version file, adding a 'Version' instruction in your descriptor or using a -Version input on the command.")
        {
        }
    }
}