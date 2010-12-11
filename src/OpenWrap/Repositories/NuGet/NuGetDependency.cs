using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using OpenWrap.Dependencies;

namespace OpenWrap.Repositories.NuGet
{
    [DataContract(Name = "dependency", Namespace = Namespaces.NuGet)]
    public class NuGetDependency
    {
        public NuGetDependency() {
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
            return new PackageDependencyBuilder(Id)
                .SetVersionVertices(CreateVersionVertices())
                .ToString();
        }

        IEnumerable<VersionVertex> CreateVersionVertices()
        {
            Version version = null;
            if (!string.IsNullOrEmpty(Version) && (version = Version.ToVersion()) != null)
                yield return new ExactVersionVertex(version);
            if (!string.IsNullOrEmpty(MinVersion) && (version = MinVersion.ToVersion()) != null)
                yield return new GreaterThenOrEqualVersionVertex(version);
            if (!string.IsNullOrEmpty(MaxVersion) && (version = MaxVersion.ToVersion()) != null)
                yield return new LessThanVersionVertex(version);
        }
    }
}