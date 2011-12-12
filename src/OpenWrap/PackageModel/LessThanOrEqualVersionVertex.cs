using System;

namespace OpenWrap.PackageModel
{
    public class LessThanOrEqualVersionVertex : VersionVertex
    {
        public LessThanOrEqualVersionVertex(SemanticVersion version)
                : base(version)
        {
        }


        public override bool IsCompatibleWith(SemanticVersion version)
        {
            return MajorMatches(version)
                   && MinorMatches(version)
                   && BuildMatches(version);
        }

        public override string ToString()
        {
            return "<= " + Version.Numeric();
        }

        bool BuildMatches(SemanticVersion version)
        {
            return Version.Patch == -1 ||
                   (version.Major < Version.Major ||
                    (version.Major == Version.Major &&
                     (version.Minor < Version.Minor ||
                      (version.Minor == Version.Minor &&
                       (version.Patch == -1 || version.Patch <= Version.Patch)))));
        }

        bool MajorMatches(SemanticVersion version)
        {
            return version.Major <= Version.Major;
        }

        bool MinorMatches(SemanticVersion version)
        {
            return version.Major < Version.Major ||
                   (version.Major == Version.Major &&
                    version.Minor <= Version.Minor);
        }
    }
    public class AproximatelyGreaterVersionVertex : VersionVertex
    {
        Func<SemanticVersion, bool> _upperBoundFunc;

        public AproximatelyGreaterVersionVertex(SemanticVersion version)
            : base(version)
        {
            if (version.Patch != -1)
                _upperBoundFunc = v => v < IncreaseNextToLast(version);
            else
                _upperBoundFunc = v => v.Major < (version.Major+1);
        }

        static SemanticVersion IncreaseNextToLast(SemanticVersion version)
        {
            var v = SemanticVersion.TryParseExact(version.ToString().Substring(0, version.ToString().LastIndexOf(".")));

            return v.Patch == -1
                       ? new SemanticVersion(v.Major, v.Minor + 1)
                       : new SemanticVersion(v.Major, v.Minor, v.Patch + 1);
        }


        public override bool IsCompatibleWith(SemanticVersion version)
        {
            return version >= Version && _upperBoundFunc(version);
        }

        public override string ToString()
        {
            return "~> " + Version.Numeric();
        }
    }
}