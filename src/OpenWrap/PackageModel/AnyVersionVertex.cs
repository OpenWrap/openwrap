using System;

namespace OpenWrap.PackageModel
{
    public class AnyVersionVertex : VersionVertex
    {
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