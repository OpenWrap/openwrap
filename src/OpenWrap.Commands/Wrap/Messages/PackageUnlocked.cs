using OpenWrap.PackageModel;

namespace OpenWrap.Commands.Wrap
{
    public class PackageUnlocked : Info
    {
        public IPackageInfo Package { get; set; }

        public PackageUnlocked(IPackageInfo package)
            : base("Package '{0}' unlocked.", package.Name)
        {
            Package = package;
        }
    }
}