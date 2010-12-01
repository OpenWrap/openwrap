using System;

namespace OpenWrap.Dependencies
{
    public class ExactVersionVertex : VersionVertex
    {
        public ExactVersionVertex(Version version) : base(version)
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
            return Version.Major == version.Major;
        }

        bool MinorMatches(Version version)
        {
            return Version.Minor == version.Minor;
        }

        bool BuildMatches(Version version)
        {
            return Version.Build == -1 || Version.Build == version.Build;
        }

        public override string ToString()
        {
            return "= " + Version;
        }
    }
}