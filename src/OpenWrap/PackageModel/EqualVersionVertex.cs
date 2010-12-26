using System;

namespace OpenWrap.PackageModel
{
    public class EqualVersionVertex : VersionVertex
    {
        public EqualVersionVertex(Version version) : base(version)
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
            return "= " + Version;
        }

        bool BuildMatches(Version version)
        {
            return Version.Build == -1 || Version.Build == version.Build;
        }

        bool MajorMatches(Version version)
        {
            return Version.Major == version.Major;
        }

        bool MinorMatches(Version version)
        {
            return Version.Minor == version.Minor;
        }
    }
}