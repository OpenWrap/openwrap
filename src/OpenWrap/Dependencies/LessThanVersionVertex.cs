using System;

namespace OpenWrap.Dependencies
{
    public class LessThanVersionVertex : VersionVertex
    {
        public LessThanVersionVertex(Version version) : base(version)
        {
            
        }

        public override bool IsCompatibleWith(Version version)
        {
            return (Version.Major == version.Major
                    && Version.Minor == version.Minor
                    && Version.Build > version.Build)
                   ||
                   (Version.Major == version.Major
                    && Version.Minor > version.Minor)
                   ||
                   Version.Major > version.Major;
        }
        public override string ToString()
        {
            return "< " + Version;
        }
    }

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

        bool BuildMatches(Version version)
        {
            return Version.Build == -1 ||
                   (version.Major < Version.Major ||
                    (version.Major == Version.Major &&
                     (version.Minor < Version.Minor ||
                      (version.Minor == Version.Minor &&
                       (version.Build == -1 || version.Build <= Version.Build)))));
        }

        public override string ToString()
        {
            return "<= " + Version;
        }
    }

}