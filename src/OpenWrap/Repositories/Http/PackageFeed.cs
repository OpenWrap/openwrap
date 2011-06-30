using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenWrap.Repositories.Http
{
    public class AtomLink
    {
        public string Rel { get; set; }
        public string Href { get; set; }
        public string Title { get; set; }
        public static implicit operator string(AtomLink atomLink)
        {
            return atomLink.Href;
        }
    }

    public class PackageFeed
    {
        public PackageFeed()
        {
            Packages = Enumerable.Empty<PackageEntry>();
            Links = Enumerable.Empty<AtomLink>().ToLookup<AtomLink,string>(x=>null);
        }
        public bool CanPublish { get; set; }
        public IEnumerable<PackageEntry> Packages { get; set; }
        public Uri BaseUri { get; set; }
        public Uri PublishHref { get; set; }
        public ILookup<string, AtomLink> Links { get; set; }
    }
}