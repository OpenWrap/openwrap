using System;

namespace OpenWrap
{
    public static class VersionExtensions
    {
        static readonly DateTime FROM = new DateTime(2010, 01, 01);

        public static SemanticVersion Numeric(this SemanticVersion version)
        {
            return new SemanticVersion(version.Major, version.Minor, version.Patch);
        }
    }
}