using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenWrap.Dependencies
{
    public interface IPackageDependency
    {
        string Name { get; }
        bool Anchored { get; }
        bool ContentOnly { get; }
        IEnumerable<string> Tags { get; set; }
        IEnumerable<VersionVertex> VersionVertices { get; }
        bool IsFulfilledBy(Version version);
    }

    public class PackageDependency : IPackageDependency
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

        }
        public string Name { get; private set; }

        public IEnumerable<VersionVertex> VersionVertices { get; private set; }

        public bool Anchored { get; private set; }

        public bool ContentOnly { get; private set; }

        public IEnumerable<string> Tags { get; set; }

        public bool IsFulfilledBy(Version version)
        {
            return VersionVertices.All(x => x.IsCompatibleWith(version));
        }
        public override string ToString()
        {
            var versions = VersionVertices.Select(x=>x.ToString()).Join(" and ");
            var returnValue = versions.Length == 0
                ? Name
                : Name + " " + versions;
            if (Tags.Count() > 0)
                returnValue += " " + string.Join(" ", Tags.ToArray());
            return returnValue;
        }

        internal bool IsExactlyFulfilledBy(Version version)
        {
            bool exactlyFulfilled = false;
            if (VersionVertices.Count() == 1)
            {
                var thisVersion = VersionVertices.FirstOrDefault();
                if (thisVersion is ExactVersionVertex)
                {
                    // only exactly fulfilled if both are of the same length
                    if (thisVersion.Version.Major
                        == version.Major
                        && thisVersion.Version.Minor
                        == version.Minor
                        && thisVersion.Version.Build
                        == version.Build
                        && thisVersion.Version.Revision == -1
                        && version.Revision == -1)
                    {
                        exactlyFulfilled = true;
                    }

                }


            }
            return exactlyFulfilled;
        }
    }

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