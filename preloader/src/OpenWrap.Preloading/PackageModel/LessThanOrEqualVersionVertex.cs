using System;

namespace OpenWrap.PackageModel
{
    public class LessThanOrEqualVersionVertex : VersionVertex
    {
        public LessThanOrEqualVersionVertex(SemanticVersion version)
                : base(version)
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
            return "<= " + Numeric(Version);
        }

        bool BuildMatches(SemanticVersion version)
        {
            return Version.Patch == -1 ||
                   (version.Major < Version.Major ||
                    (version.Major == Version.Major &&
                     (version.Minor < Version.Minor ||
                      (version.Minor == Version.Minor &&
                       (version.Patch == -1 || version.Patch <= Version.Patch)))));
        }

        bool MajorMatches(SemanticVersion version)
        {
            return version.Major <= Version.Major;
        }

        bool MinorMatches(SemanticVersion version)
        {
            return version.Major < Version.Major ||
                   (version.Major == Version.Major &&
                    version.Minor <= Version.Minor);
        }
    }
}