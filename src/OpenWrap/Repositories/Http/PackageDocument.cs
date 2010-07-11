using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenWrap.Repositories.Http
{
    public class PackageDocument
    {
        public IEnumerable<PackageItem> Packages { get; set; }
    }

    public class PackageItem
    {
        public string Name { get; set; }
        public Version Version { get; set; }
        public DateTime? LastModifiedTimeUtc { get; set; }
        public Uri PackageHref { get; set; }
        public IEnumerable<string> Dependencies { get; set; }
    }
}
