﻿using System;

namespace OpenWrap.PackageModel
{
    public class PackageIdentifier : IEquatable<PackageIdentifier>
    {
        public PackageIdentifier(string name) : this(name, null)
        {
        }

        public PackageIdentifier(string name, SemanticVersion version)
        {
            if (name == null) throw new ArgumentNullException("name");
            Name = name;
            Version = version;
        }

        public string Name { get; private set; }
        public SemanticVersion Version { get; private set; }

        public static bool operator ==(PackageIdentifier left, PackageIdentifier right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PackageIdentifier left, PackageIdentifier right)
        {
            return !Equals(left, right);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(PackageIdentifier)) return false;
            return Equals((PackageIdentifier)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ (Version != null ? Version.GetHashCode() : 0);
            }
        }

        public bool IsCompatibleWith(PackageDependency dependency)
        {
            return Name.EqualsNoCase(dependency.Name)
                   && dependency.IsFulfilledBy(Version);
        }

        public override string ToString()
        {
            return Name + (Version == null ? string.Empty : "-" + Version);
        }

        public bool Equals(PackageIdentifier other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Name, Name) && Equals(other.Version, Version);
        }
    }
}