using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenWrap.Dependencies
{
    public class PackageDependencyBuilder
    {
        List<string> _tags;
        string _name;
        List<VersionVertex> _versions;

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
        public PackageDependencyBuilder AddTag(string tag)
        {
            var index = _tags.FindIndex(x => x.EqualsNoCase(tag));
            if (index == -1)
                _tags.Add(tag);
            return this;
        }
        public PackageDependencyBuilder RemoveTag(string tag)
        {
            var index = _tags.FindIndex(x => x.EqualsNoCase(tag));
            if (index != -1)
                _tags.RemoveAt(index);
            return this;
        }

        public PackageDependencyBuilder VersionVertex(VersionVertex vertex)
        {
            _versions.Add(vertex);
            return this;
        }
        public PackageDependencyBuilder SetVersionVertices(IEnumerable<VersionVertex> vertices)
        {
            _versions.Clear();
            _versions.AddRange(vertices);
            return this;
        }
        void SetTagValue(string tag, bool isSet)
        {
            if (isSet && !ContainsTag(tag))
                _tags.Add(tag);
            else if (!isSet && ContainsTag(tag))
                _tags.Remove(tag);
        }

        bool ContainsTag(string tag)
        {
            return _tags.Contains(tag, StringComparer.OrdinalIgnoreCase);
        }

        public static implicit operator PackageDependency(PackageDependencyBuilder builder)
        {
            return new PackageDependency(builder._name, builder._versions, builder._tags);
        }

        public PackageDependencyBuilder Name(string name)
        {
            _name = name;
            return this;
        }
        public override string ToString()
        {
            return ((PackageDependency)this).ToString();
        }
    }
}