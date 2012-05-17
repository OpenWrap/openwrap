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
        public static SemanticVersion ToSemVer(this Version version)
        {
            if (version == null) return null;
            var ver = new SemanticVersion(
                version.Major, 
                version.Minor,
                version.Build,
                build: version.Revision < 0 ? null : version.Revision.ToString());
            return ver;
        }
    }
}