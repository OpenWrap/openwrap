using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel.Syndication;
using System.Text;
using OpenWrap.Dependencies;
using OpenWrap.Repositories.Http;

namespace OpenWrap.Repositories.NuPack
{
    public static class Namespaces
    {
        public const string NuPack = "http://schemas.microsoft.com/packaging/2010/07/";
    }
    public class NuPackSyndicationFeed : SyndicationFeed
    {
        protected override SyndicationItem CreateItem()
        {
            return new NuPackSyndicationItem();
        }
        public PackageDocument ToPackageDocument()
        {
            return new PackageDocument
            {
                    CanPublish = false,
                    Packages = this.Items.Cast<NuPackSyndicationItem>().Select(x => x.ToPackage()).ToList()
            };
        }
    }

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
    [DataContract(Name = "dependency", Namespace = Namespaces.NuPack)]
    public class NuPackDependency
    {
        public NuPackDependency() {
        }
        [DataMember(Name = "id", EmitDefaultValue = false)]
        public string Id { get; set; }

        [DataMember(Name = "version", EmitDefaultValue = false)]
        public string Version { get; set; }

        [DataMember(Name = "minVersion", EmitDefaultValue = false)]
        public string MinVersion { get; set; }

        [DataMember(Name = "maxVersion", EmitDefaultValue = false)]
        public string MaxVersion { get; set; }

        public string ToPackageDependencyLine()
        {
            return "depends: " + new PackageDependency
            {
                    Name = Id,
                    VersionVertices = CreateVersionVertices().ToList()
            }.ToString();
        }

        IEnumerable<VersionVertex> CreateVersionVertices()
        {
            Version version = null;
            if (!string.IsNullOrEmpty(Version) && (version = Version.ToVersion()) != null)
                yield return new ExactVersionVertex(version);
            if (!string.IsNullOrEmpty(MinVersion) && (version = MinVersion.ToVersion()) != null)
                yield return new GreaterThenOrEqualVersionVertex(version);
            if (!string.IsNullOrEmpty(MinVersion) && (version = MaxVersion.ToVersion()) != null)
                yield return new LessThanVersionVertex(version);
        }
    }
    public static class NuPackAtomExtensions
    {
        public static T Extension<T>(this SyndicationElementExtensionCollection extensions, string name)
        {
            return extensions.ReadNuPackExtension<T>(name).First();
        }

        public static T OptionalExtension<T>(this SyndicationElementExtensionCollection extensions, string name)
        {
            return extensions.ReadNuPackExtension<T>(name).FirstOrDefault();
        }
        static Collection<T> ReadNuPackExtension<T>(this SyndicationElementExtensionCollection extensions, string name)
        {
            return extensions.ReadElementExtensions<T>(name, Namespaces.NuPack);
        }
    }
}
