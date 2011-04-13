using System;
using OpenWrap.Build;

namespace OpenWrap.PackageModel
{
    public class GreaterThanVersionVertex : VersionVertex
    {
        public GreaterThanVersionVertex(Version version) : base(version)
        {
        }

        public override bool IsCompatibleWith(Version version)
        {
            return (Version.Major == version.Major
                    && Version.Minor == version.Minor
                    && Version.Build < version.Build)
                   ||
                   (Version.Major == version.Major
                    && Version.Minor < version.Minor)
                   ||
                   (Version.Major < version.Major);
        }

        public override string ToString()
        {
            return "> " + Version.IgnoreRevision();
        }
    }
}