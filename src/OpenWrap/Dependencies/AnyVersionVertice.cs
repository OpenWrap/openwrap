using System;

namespace OpenRasta.Wrap.Dependencies
{
    public class AnyVersionVertice : VersionVertice
    {
        public AnyVersionVertice(Version version) : base(version)
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