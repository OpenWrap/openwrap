using System;

namespace OpenWrap.Dependencies
{
    public class AnyVersionVertex : VersionVertex
    {
        public AnyVersionVertex(Version version) : base(version)
        {
        }
        public AnyVersionVertex() : base(new Version())
        {
        }
        public override bool IsCompatibleWith(Version version)
        {
            return true;
        }
        public override string ToString()
        {
            return string.Empty;
        }
    }
}