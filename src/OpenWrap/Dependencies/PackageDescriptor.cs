using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Collections;
using OpenWrap.Repositories;

namespace OpenWrap.Dependencies
{
    public class DescriptorLineCollection : IEnumerable<IPackageDescriptorLine>
    {
        readonly Dictionary<string, List<GenericDescriptorLine>> _byName = new Dictionary<string, List<GenericDescriptorLine>>(StringComparer.OrdinalIgnoreCase);
        readonly List<GenericDescriptorLine> _headers = new List<GenericDescriptorLine>();

        public DescriptorLineCollection(IEnumerable<KeyValuePair<string, string>> descriptorLines)
        {
            foreach (var value in descriptorLines)
                Append(value.Key, value.Value);
        }

        public DescriptorLineCollection()
        {
        }

        public IPackageDescriptorLine Append(string name, string value)
        {
            var header = new GenericDescriptorLine(name, value);
            if (!_headers.Contains(header))
                _headers.Add(header);
            GetOrAddValueList(name).Add(header);
            return header;
        }

        public void Remove(string name)
        {
            var headers = GetOrAddValueList(name);
            _headers.RemoveRange(headers);
            headers.Clear();
        }

        public void Remove(string name, string value)
        {
            var headerValue = new GenericDescriptorLine(name, value);
            _headers.Remove(headerValue);
            GetOrAddValueList(name).Remove(headerValue);
        }

        public void Set(string name, string value)
        {
            // want to keep the order of headers, need to choose the first found one
            var allHeadersWithSameName = _headers.Where(x => x.Name.EqualsNoCase(name))
                                                 .ToList();

            var entryToEdit = allHeadersWithSameName.FirstOrDefault();
            var presentHeaderIndex = _headers.IndexOf(entryToEdit);
            var toRemove = allHeadersWithSameName.ToList();

            _headers.RemoveRange(toRemove);

            var headerValues = GetOrAddValueList(name);
            headerValues.Clear();

            var header = new GenericDescriptorLine(name, value);

            if (presentHeaderIndex == -1)
                _headers.Add(header);
            else
                _headers.Insert(presentHeaderIndex, header);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<IPackageDescriptorLine> GetEnumerator()
        {
            foreach (var val in _headers)
                yield return val;
        }

        List<GenericDescriptorLine> GetOrAddValueList(string name)
        {
            List<GenericDescriptorLine> values;
            if (!_byName.TryGetValue(name, out values))
                _byName[name] = values = new List<GenericDescriptorLine>();
            return values;
        }
    }

    public class GenericDescriptorLine : IEquatable<GenericDescriptorLine>, IPackageDescriptorLine
    {
        readonly string _nameForComparison;

        public GenericDescriptorLine(string name, string value)
        {
            Name = name;
            _nameForComparison = name.ToLowerInvariant();
            Value = value;
        }

        public string Name { get; set; }
        public string Value { get; set; }

        public static bool operator ==(GenericDescriptorLine left, GenericDescriptorLine right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(GenericDescriptorLine left, GenericDescriptorLine right)
        {
            return !Equals(left, right);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(GenericDescriptorLine)) return false;
            return Equals((GenericDescriptorLine)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_nameForComparison != null ? _nameForComparison.GetHashCode() : 0) * 397) ^ (Value != null ? Value.GetHashCode() : 0);
            }
        }

        public bool Equals(GenericDescriptorLine other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._nameForComparison, _nameForComparison) && Equals(other.Value, Value);
        }
    }

    public class PackageDescriptor : IEnumerable<IPackageDescriptorLine>
    {
        readonly DescriptorLineCollection _lines = new DescriptorLineCollection();
        SingleBoolValue _anchored;
        SingleStringValue _buildCommand;
        SingleDateTimeOffsetValue _created;
        PackageDependencyCollection _dependencies;
        SingleStringValue _description;
        SingleStringValue _name;
        SingleBoolValue _useProjectRepository;
        SingleVersionValue _version;
        SingleBoolValue _useSymLinks;
        PackageNameOverrideCollection _overrides;

        public PackageDescriptor(IEnumerable<IPackageDescriptorLine> lines)
        {
            foreach (var line in lines)
                _lines.Append(line.Name, line.Value);
            InitializeHeaders();
        }

        public PackageDescriptor()
        {
            InitializeHeaders();
        }

        void InitializeHeaders()
        {
            _dependencies = new PackageDependencyCollection(_lines);
            _overrides = new PackageNameOverrideCollection(_lines);

            _anchored = new SingleBoolValue(_lines, "anchored", false);
            _buildCommand = new SingleStringValue(_lines, "build");
            _created = new SingleDateTimeOffsetValue(_lines, "created");
            _description = new SingleStringValue(_lines, "description");
            _name = new SingleStringValue(_lines, "name");
            _version = new SingleVersionValue(_lines, "version");
            _useProjectRepository = new SingleBoolValue(_lines, "use-project-repository", true);
            _useSymLinks = new SingleBoolValue(_lines, "use-symlinks", true);
        }

        public bool Anchored
        {
            get { return _anchored.Value; }
            set { _anchored.Value = value; }
        }

        public string BuildCommand
        {
            get { return _buildCommand.Value; }
            set { _buildCommand.Value = value; }
        }

        public DateTimeOffset Created
        {
            get { return _created.Value; }
            private set { _created.Value = value; }
        }

        public ICollection<PackageDependency> Dependencies { get { return _dependencies; } }

        public string Description
        {
            get { return _description.Value; }
            set { _description.Value = value; }
        }

        public string FullName
        {
            get { return Name + "-" + Version; }
        }

        public PackageIdentifier Identifier
        {
            get { return new PackageIdentifier(Name, Version); }
        }

        public string Name
        {
            get { return _name.Value; }
            set { _name.Value = value; }
        }

        public bool Nuked
        {
            get { return false; }
        }

        public ICollection<PackageNameOverride> Overrides { get { return _overrides; } }

        public IPackageRepository Source
        {
            get { return null; }
        }

        public bool UseProjectRepository
        {
            get { return _useProjectRepository.Value; }
            set { _useProjectRepository.Value = value; }
        }

        public Version Version
        {
            get { return _version.Value; }
            set { _version.Value = value; }
        }

        public bool UseSymLinks
        {
            get { return _useSymLinks.Value; }
            set { _useSymLinks.Value = value; }
        }

        public IPackage Load()
        {
            return null;
        }

        public IEnumerator<IPackageDescriptorLine> GetEnumerator()
        {
            foreach(var line in _lines)
                yield return line;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}