using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using OpenWrap.Repositories.Http;

namespace OpenWrap.Repositories.NuPack
{
    public class NuPackSyndicationItem : SyndicationItem
    {
        public string PackageName
        {
            get { return ElementExtensions.Extension<string>("packageId"); }
        }
        public string PackageVersion
        {
            get { return ElementExtensions.Extension<string>("version"); }
        }

        public Uri PackageHref
        {
            get { return Links.Where(x => x.RelationshipType.Equals("enclosure", StringComparison.OrdinalIgnoreCase)).First().GetAbsoluteUri(); }
        }
        public List<string> Dependencies { get
        {
            var deps = ElementExtensions.OptionalExtension<NuPackDependency[]>("dependencies")
                       ?? Enumerable.Empty<NuPackDependency>();

            return deps.Select(x=>x.ToPackageDependencyLine()).ToList();
        }
        }

        public PackageItem ToPackage()
        {
            return new PackageItem
            {
                    Dependencies = Dependencies,
                    Name = PackageName,
                    Version = PackageVersion.ToVersion(),
                    PackageHref = PackageHref,
                    LastModifiedTimeUtc = this.PublishDate.UtcDateTime
            };
        }
    }
}