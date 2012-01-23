using OpenWrap.PackageModel;

namespace OpenWrap.Commands.Wrap
{
    public class PackageLocked : Info
    {
        public IPackageInfo Package { get; set; }

        public PackageLocked(IPackageInfo package) : base("Package '{0}' locked at version '{1}'.", package.Name, package.SemanticVersion)
        {
            Package = package;
        }
    }
}