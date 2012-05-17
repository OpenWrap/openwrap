namespace OpenWrap.Commands.Wrap
{
    public class PackageLockedNotRemoved : Error
    {
        public string PackageName { get; set; }

        public PackageLockedNotRemoved(string packageName) : base("Cannont remove package '{0}' as it is currently locked. Unlock the package with 'unlock-wrap {0}' first, then remove.", packageName)
        {
            PackageName = packageName;
        }
    }
}