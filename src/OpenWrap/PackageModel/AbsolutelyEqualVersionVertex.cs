using System;

namespace OpenWrap.PackageModel
{
    public class AbsolutelyEqualVersionVertex : VersionVertex
    {
        public AbsolutelyEqualVersionVertex(SemanticVersion version) : base(version)
        {
        }

        public override bool IsCompatibleWith(SemanticVersion version)
        {
            return Version.Equals(version);
        }
        public override string ToString()
        {
            return "≡ " + Version;
        }
    }
}