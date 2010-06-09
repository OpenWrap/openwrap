using System;

namespace OpenWrap.Dependencies
{
    public class AnyVersionVertice : VersionVertice
    {
        public AnyVersionVertice(Version version) : base(version)
        {
        }
        public AnyVersionVertice() : base(new Version())
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