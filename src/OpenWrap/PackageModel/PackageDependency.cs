using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenWrap.PackageModel
{
    public class PackageDependency : IEquatable<PackageDependency>
    {
        public PackageDependency(
                string name,
                IEnumerable<VersionVertex> versions = null,
                IEnumerable<string> tags = null)
        {
            Name = name;
            Tags = tags != null ? new List<string>(tags).AsReadOnly() : Enumerable.Empty<string>();
            VersionVertices = versions != null ? new List<VersionVertex>(versions).AsReadOnly() : Enumerable.Empty<VersionVertex>();
            ContentOnly = Tags.ContainsNoCase("content");
            Anchored = Tags.ContainsNoCase("anchored");
            Edge = Tags.ContainsNoCase("edge");
        }

        public bool Edge { get; private set; }

        public bool Anchored { get; private set; }

        public bool ContentOnly { get; private set; }
        public string Name { get; private set; }

        public IEnumerable<string> Tags { get; set; }

        // TODO: Remove this implementation detail.
        public IEnumerable<VersionVertex> VersionVertices { get; private set; }

        public override string ToString()
        {
            return Name.AppendSpace(VersionVertices.Select(x => x.ToString()).JoinString(" and "))
                .AppendSpace(Tags.JoinString(" "));
        }

        public bool IsFulfilledBy(SemanticVersion version)
        {
            return VersionVertices.All(x => x.IsCompatibleWith(version))
                && (Edge || (!Edge && version.PreRelease == null));
        }

        public bool Equals(PackageDependency other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.ToString(), ToString());
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(PackageDependency)) return false;
            return Equals((PackageDependency)obj);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public static bool operator ==(PackageDependency left, PackageDependency right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PackageDependency left, PackageDependency right)
        {
            return !Equals(left, right);
        }
    }
}