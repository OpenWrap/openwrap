using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using OpenWrap.Dependencies;

namespace OpenWrap.Repositories.NuPack
{
    [DataContract(Name="properties", Namespace=Namespaces.AstoriaD)]
    public class NuPackODataProperties
    {
        
        public string Version { get; set; }
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
            return new PackageDependency
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
}