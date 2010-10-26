using System;

namespace OpenWrap.Dependencies
{
    public abstract class VersionVertex
    {
        public Version Version { get; private set; }

        protected VersionVertex(Version version)
        {
            if (version == null) throw new ArgumentNullException("version");
            Version = version;
        }

        protected VersionVertex()
        {
            
        }

        public abstract bool IsCompatibleWith(Version version);
    }
}