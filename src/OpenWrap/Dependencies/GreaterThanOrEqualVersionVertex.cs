using System;

namespace OpenWrap.Dependencies
{
    public class GreaterThanOrEqualVersionVertex : VersionVertex
    {
        public GreaterThanOrEqualVersionVertex(Version version)
            : base(version)
        {
        }

        public override bool IsCompatibleWith(Version version)
        {
            return MajorMatches(version)
                   && MinorMatches(version)
                   && BuildMatches(version);
        }

        bool MajorMatches(Version version)
        {
            return version.Major >= Version.Major;
        }

        bool MinorMatches(Version version)
        {
            return version.Major > Version.Major ||
                   (version.Major == Version.Major &&
                    version.Minor >= Version.Minor);
        }

        bool BuildMatches(Version version)
        {
            return Version.Build == -1 ||
                   (version.Major > Version.Major ||
                    (version.Major == Version.Major &&
                     (version.Minor > Version.Minor ||
                      (version.Minor == Version.Minor &&
                       (version.Build == -1 || version.Build >= Version.Build)))));
        }

        public override string ToString()
        {
            return ">= " + Version;
        }
    }
}