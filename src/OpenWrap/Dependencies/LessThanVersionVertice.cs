using System;

namespace OpenWrap.Dependencies
{
    public class LessThanVersionVertice : VersionVertice
    {
        public LessThanVersionVertice(Version version) : base(version)
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
    }
}