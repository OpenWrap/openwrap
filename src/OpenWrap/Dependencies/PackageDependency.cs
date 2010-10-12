using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenWrap.Dependencies
{
    public class PackageDependency
    {
        public PackageDependency(PackageDependency copy)
        {
            VersionVertices = new List<VersionVertex>(copy.VersionVertices);
            Tags = new List<string>(copy.Tags);
        }
        public PackageDependency()
        {
            VersionVertices = new List<VersionVertex>();
            Tags = new List<string>();
        }
        public string Name { get; set; }
        public ICollection<VersionVertex> VersionVertices { get; set; }

        public bool Anchored
        {
            get { return Tags.Contains("anchored", StringComparer.OrdinalIgnoreCase);}
            set { SetTag("anchored", value); }
        }

        public bool ContentOnly
        {
            get { return Tags.Contains("content", StringComparer.OrdinalIgnoreCase); }
            set { SetTag("content", value); }

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

        bool IsExact()
        {
            return VersionVertices.Count() == 1 
                && VersionVertices.First() is ExactVersionVertex;
        }
    }
}