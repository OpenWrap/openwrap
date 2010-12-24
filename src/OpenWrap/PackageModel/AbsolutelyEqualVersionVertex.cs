using System;

namespace OpenWrap.PackageModel
{
    public class AbsolutelyEqualVersionVertex : VersionVertex
    {
        public AbsolutelyEqualVersionVertex(Version version) : base(version)
        {
        }

        public override bool IsCompatibleWith(Version version)
        {
            return Version.Equals(version);
        }
    }
}