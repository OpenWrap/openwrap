using System;

namespace OpenWrap.PackageModel
{
    public abstract class VersionVertex
    {
        protected VersionVertex(Version version)
        {
            if (version == null) throw new ArgumentNullException("version");
            Version = version;
        }

        protected VersionVertex()
        {
        }

        public Version Version { get; private set; }

        public abstract bool IsCompatibleWith(Version version);
    }
}