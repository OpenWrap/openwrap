using System;
using System.Collections.Generic;

namespace OpenWrap.Repositories.Http
{
    public class PackageItem
    {
        public string Name { get; set; }
        public Version Version { get; set; }
        public DateTimeOffset CreationTime { get; set; }
        public Uri PackageHref { get; set; }
        public IEnumerable<string> Dependencies { get; set; }

        public string Description { get; set; }
        public bool Nuked { get; set; }
    }
}