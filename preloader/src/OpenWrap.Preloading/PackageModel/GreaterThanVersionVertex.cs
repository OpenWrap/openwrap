using System;

namespace OpenWrap.PackageModel
{
    public class GreaterThanVersionVertex : VersionVertex
    {
        public GreaterThanVersionVertex(SemanticVersion version)
            : base(version)
        {
        }

        public override bool IsCompatibleWith(SemanticVersion version)
        {
            return (Version.Major == version.Major
                    && Version.Minor == version.Minor
                    && Version.Patch < version.Patch)
                   ||
                   (Version.Major == version.Major
                    && Version.Minor < version.Minor)
                   ||
                   (Version.Major < version.Major);
        }

        public override string ToString()
        {
            return "> " + Numeric(Version);
        }
    }
}