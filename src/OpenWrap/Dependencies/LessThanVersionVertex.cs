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
}