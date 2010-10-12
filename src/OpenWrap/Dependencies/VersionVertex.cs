using System;

namespace OpenWrap.Dependencies
{
    public abstract class VersionVertex
    {
        public Version Version { get; private set; }

        public VersionVertex(Version version)
        {
            Version = version;
        }
        public abstract bool IsCompatibleWith(Version version);
    }
}