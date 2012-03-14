using System;

namespace OpenWrap.PackageModel
{
    public class EqualVersionVertex : VersionVertex
    {
        public EqualVersionVertex(SemanticVersion version) : base(version)
        {
        }

        public override bool IsCompatibleWith(SemanticVersion version)
        {
            return MajorMatches(version)
                   && MinorMatches(version)
                   && BuildMatches(version);
        }

        public override string ToString()
        {
            return "= " + Numeric(Version);
        }

        bool BuildMatches(SemanticVersion version)
        {
            return Version.Patch == -1 || Version.Patch == version.Patch;
        }

        bool MajorMatches(SemanticVersion version)
        {
            return Version.Major == version.Major;
        }

        bool MinorMatches(SemanticVersion version)
        {
            return Version.Minor == -1 || Version.Minor == version.Minor;
        }
    }
}