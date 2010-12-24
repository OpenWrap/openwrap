using System;
using System.Collections.Generic;

namespace OpenWrap.Repositories.Http
{
    public class PackageDocument
    {
        public bool CanPublish { get; set; }
        public IEnumerable<PackageItem> Packages { get; set; }

        public Uri PublishHref { get; set; }
    }
}