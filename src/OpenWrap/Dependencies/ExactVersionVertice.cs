using System;

namespace OpenWrap.Dependencies
{
    public class ExactVersionVertice : VersionVertice
    {
        public ExactVersionVertice(Version version) : base(version)
        {
            
        }

        public override bool IsCompatibleWith(Version version)
        {
            return Version.Major == version.Major
                   && Version.Minor == version.Minor
                   && Version.Build <= version.Build;
        }
        public override string ToString()
        {
            return "= " + Version;
        }
    }
}