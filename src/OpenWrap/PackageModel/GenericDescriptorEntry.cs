using System;

namespace OpenWrap.PackageModel
{
    public class GenericDescriptorEntry : IEquatable<GenericDescriptorEntry>, IPackageDescriptorEntry
    {
        readonly string _nameForComparison;

        public GenericDescriptorEntry(string name, string value)
        {
            Name = name;
            _nameForComparison = name.ToLowerInvariant();
            Value = value;
        }

        public string Name { get; set; }
        public string Value { get; set; }

        public static bool operator ==(GenericDescriptorEntry left, GenericDescriptorEntry right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(GenericDescriptorEntry left, GenericDescriptorEntry right)
        {
            return !Equals(left, right);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(GenericDescriptorEntry)) return false;
            return Equals((GenericDescriptorEntry)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_nameForComparison != null ? _nameForComparison.GetHashCode() : 0) * 397) ^ (Value != null ? Value.GetHashCode() : 0);
            }
        }

        public bool Equals(GenericDescriptorEntry other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._nameForComparison, _nameForComparison) && Equals(other.Value, Value);
        }
    }
}