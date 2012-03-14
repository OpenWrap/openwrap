using System;

namespace OpenWrap.PackageModel
{
    public abstract class VersionVertex
    {
        protected VersionVertex(SemanticVersion version)
        {
            if (version == null) throw new ArgumentNullException("version");
            Version = version;
        }

        protected VersionVertex()
        {
        }

        public SemanticVersion Version { get; private set; }

        public abstract bool IsCompatibleWith(SemanticVersion version);

        protected SemanticVersion Numeric(SemanticVersion version)
        {
            return new SemanticVersion(version.Major, version.Minor, version.Patch);
        }
    }
}