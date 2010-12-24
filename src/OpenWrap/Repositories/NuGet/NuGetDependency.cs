using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using OpenWrap.PackageModel;

namespace OpenWrap.Repositories.NuGet
{
    [DataContract(Name = "dependency", Namespace = Namespaces.NuGet)]
    public class NuGetDependency
    {
        [DataMember(Name = "version", EmitDefaultValue = false)]
        public string ExactVersion { get; set; }

        [DataMember(Name = "id", EmitDefaultValue = false)]
        public string Id { get; set; }

        [DataMember(Name = "maxVersion", EmitDefaultValue = false)]
        public string MaxVersion { get; set; }

        [DataMember(Name = "minVersion", EmitDefaultValue = false)]
        public string MinVersion { get; set; }

        public string Version { get; set; }

        public string ToPackageDependencyLine()
        {
            return new PackageDependencyBuilder(Id)
                    .SetVersionVertices(CreateVersionVertices())
                    .ToString();
        }

        IEnumerable<VersionVertex> CreateVersionVertices()
        {
            if (Version != null)
            {
                foreach (var vertice in NuSpecConverter.ConvertNuGetVersionRange(Version).DefaultIfEmpty(new AnyVersionVertex()))
                    yield return vertice;
                yield break;
            }
            Version version = null;
            if (!string.IsNullOrEmpty(ExactVersion) && (version = ExactVersion.ToVersion()) != null)
                yield return new EqualVersionVertex(version);
            if (!string.IsNullOrEmpty(MinVersion) && (version = MinVersion.ToVersion()) != null)
                yield return new GreaterThanOrEqualVersionVertex(version);
            if (!string.IsNullOrEmpty(MaxVersion) && (version = MaxVersion.ToVersion()) != null)
                yield return new LessThanVersionVertex(version);
        }
    }
}