using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenWrap.Dependencies
{
    public class WrapDependency
    {
        public WrapDependency()
        {
            VersionVertices = new List<VersionVertice>();
        }
        public string Name { get; set; }
        public ICollection<VersionVertice> VersionVertices { get; set; }

        public bool Anchored
        {
            get { return Tags.Contains("anchored", StringComparer.OrdinalIgnoreCase);}
            set { if (!Tags.Contains("anchored")) Tags.Add("anchored"); }
        }

        public bool ContentOnly
        {
            get { return Tags.Contains("content", StringComparer.OrdinalIgnoreCase); }
            set { if (!Tags.Contains("content")) Tags.Add("content"); }

        }
        void SetTag(string tag, bool isSet)
        {
            if (isSet && !Tags.Contains(tag))
                Tags.Add(tag);
            else if (!isSet && Tags.Contains(tag))
                Tags.Remove(tag);
        }
        public ICollection<string> Tags { get; set; }

        public bool IsFulfilledBy(Version version)
        {
            return VersionVertices.All(x => x.IsCompatibleWith(version));
        }
        public override string ToString()
        {
            var versions = string.Join(" and ", VersionVertices.Select(x => x.ToString()).ToArray());
            var returnValue = versions.Length == 0
                ? Name
                : Name + " " + versions;
            if (Tags.Count() > 0)
                returnValue = string.Join(" ", Tags.ToArray());
            return returnValue;
        }
    }
}