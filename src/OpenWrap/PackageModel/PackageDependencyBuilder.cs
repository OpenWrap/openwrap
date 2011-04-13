using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenWrap.PackageModel
{
    public class PackageDependencyBuilder
    {
        readonly List<string> _tags;
        readonly List<VersionVertex> _versions;
        string _name;

        public PackageDependencyBuilder(PackageDependency dep)
        {
            _name = dep.Name;
            _tags = new List<string>(dep.Tags);
            _versions = new List<VersionVertex>(dep.VersionVertices);
        }

        public PackageDependencyBuilder(string name)
        {
            _name = name;
            _tags = new List<string>();
            _versions = new List<VersionVertex>();
        }

        public static implicit operator PackageDependency(PackageDependencyBuilder builder)
        {
            return builder.Build();
        }

        public PackageDependencyBuilder AddTag(string tag)
        {
            var index = _tags.FindIndex(x => x.EqualsNoCase(tag));
            if (index == -1)
                _tags.Add(tag);
            return this;
        }

        public PackageDependencyBuilder Anchored(bool isAnchored = true)
        {
            SetTagValue("anchored", isAnchored);
            return this;
        }

        public PackageDependencyBuilder Content(bool isContent = true)
        {
            SetTagValue("content", isContent);
            return this;
        }

        public PackageDependencyBuilder Name(string name)
        {
            _name = name;
            return this;
        }

        public PackageDependencyBuilder RemoveTag(string tag)
        {
            var index = _tags.FindIndex(x => x.EqualsNoCase(tag));
            if (index != -1)
                _tags.RemoveAt(index);
            return this;
        }
        public PackageDependencyBuilder Version(string version)
        {
            _versions.Add(new EqualVersionVertex(version.ToVersion()));
            return this;
        }

        public PackageDependencyBuilder MinVersion(string version)
        {
            _versions.Add(new GreaterThanOrEqualVersionVertex(version.ToVersion()));
            return this;
        }
        public PackageDependencyBuilder MaxVersion(string version)
        {
            _versions.Add(new LessThanVersionVertex(version.ToVersion()));
            return this;
        }
        public PackageDependencyBuilder SetVersionVertices(IEnumerable<VersionVertex> vertices)
        {
            _versions.Clear();
            _versions.AddRange(vertices);
            return this;
        }

        public override string ToString()
        {
            return ((PackageDependency)this).ToString();
        }

        public PackageDependencyBuilder VersionVertex(VersionVertex vertex)
        {
            _versions.Add(vertex);
            return this;
        }

        bool ContainsTag(string tag)
        {
            return _tags.Contains(tag, StringComparer.OrdinalIgnoreCase);
        }

        void SetTagValue(string tag, bool isSet)
        {
            if (isSet && !ContainsTag(tag))
                _tags.Add(tag);
            else if (!isSet && ContainsTag(tag))
                _tags.Remove(tag);
        }

        public PackageDependency Build()
        {
            return new PackageDependency(_name, _versions, _tags);
        }
    }
}