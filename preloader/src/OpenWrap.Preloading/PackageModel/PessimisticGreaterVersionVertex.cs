using System;

namespace OpenWrap.PackageModel
{
    public class PessimisticGreaterVersionVertex : VersionVertex
    {
        public PessimisticGreaterVersionVertex(SemanticVersion version)
            : base(version)
        {
            MaxVersion = IncreaseNextToLast(version);
        }

        public SemanticVersion MaxVersion { get; private set; }


        public override bool IsCompatibleWith(SemanticVersion version)
        {
            return version >= Version && Numeric(version) < MaxVersion;
        }

        public override string ToString()
        {
            return "~> " + Numeric(Version);
        }

        static SemanticVersion IncreaseNextToLast(SemanticVersion version)
        {
            return new SemanticVersion(
                (version.Minor == -1 || version.Patch == -1) ? version.Major + 1 : version.Major, 
                version.Patch == -1 ? -1 : version.Minor + 1);
        }
    }
}