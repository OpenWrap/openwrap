using System.Collections.Generic;
using OpenWrap.Configuration;
using OpenWrap.Repositories.Http;

namespace OpenWrap.Repositories.Caching
{
    public class PackageEntryContainer
    {
        public PackageEntryContainer()
        {
            Packages = new List<PackageEntry>();

        }
        [Key("package")]
        public List<PackageEntry> Packages { get; set; }
    }
}