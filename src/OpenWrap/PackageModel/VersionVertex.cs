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
    }
}