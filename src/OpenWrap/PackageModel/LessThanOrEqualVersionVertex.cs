using System;

namespace OpenWrap.PackageModel
{
    public class LessThanOrEqualVersionVertex : VersionVertex
    {
        public LessThanOrEqualVersionVertex(Version version)
                : base(version)
        {
        }


        public override bool IsCompatibleWith(Version version)
        {
            return MajorMatches(version)
                   && MinorMatches(version)
                   && BuildMatches(version);
        }

        public override string ToString()
        {
            return "<= " + Version;
        }

        bool BuildMatches(Version version)
        {
            return Version.Build == -1 ||
                   (version.Major < Version.Major ||
                    (version.Major == Version.Major &&
                     (version.Minor < Version.Minor ||
                      (version.Minor == Version.Minor &&
                       (version.Build == -1 || version.Build <= Version.Build)))));
        }

        bool MajorMatches(Version version)
        {
            return version.Major <= Version.Major;
        }

        bool MinorMatches(Version version)
        {
            return version.Major < Version.Major ||
                   (version.Major == Version.Major &&
                    version.Minor <= Version.Minor);
        }
    }
}