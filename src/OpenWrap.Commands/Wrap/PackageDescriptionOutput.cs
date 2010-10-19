using System.Collections.Generic;
using System.Linq;
using OpenWrap.Repositories;

namespace OpenWrap.Commands.Wrap
{
    public class PackageDescriptionOutput : GenericMessage
    {
        public string PackageName { get; set; }

        public PackageDescriptionOutput(string packageName, IEnumerable<IPackageInfo> packageVersions)
                : base(" - {0} (versions: {1})", packageName, CreateVersionOutput(packageVersions))
        {
            PackageName = packageName;
        }

        static string CreateVersionOutput(IEnumerable<IPackageInfo> packageVersions)
        {
            return packageVersions.Select(VersionIdentifier).Join(", ");
        }

        static string VersionIdentifier(IPackageInfo x)
        {
            return x.Version + (x.Nuked ? " [nuked]" : string.Empty);
        }
    }
}