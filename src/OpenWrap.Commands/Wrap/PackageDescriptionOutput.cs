using System.Collections.Generic;
using System.Linq;
using OpenWrap.Repositories;

namespace OpenWrap.Commands.Wrap
{
    public class PackageDescriptionOutput : GenericMessage
    {
        public string PackageName { get; set; }

        public PackageDescriptionOutput(string packageName, IEnumerable<IPackageInfo> packageVersions)
                : base(" - {0}\r\n   Versions: {1}", packageName, string.Join(", ", packageVersions.Select(x => x.Version.ToString()).ToArray()))
        {
            PackageName = packageName;
        }
    }
}