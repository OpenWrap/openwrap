using System;

namespace OpenWrap.PackageModel
{
    public class LessThanOrEqualVersionVertex : VersionVertex
    {
        public LessThanOrEqualVersionVertex(Version version)
                : base(version)
        {
        }


        public override bool IsCompatibleWith(Version version)
        {
            return MajorMatches(version)
                   && MinorMatches(version)
                   && BuildMatches(version);
        }

        public override string ToString()
        {
            return "<= " + Version.IgnoreRevision();
        }

        bool BuildMatches(Version version)
        {
            return Version.Build == -1 ||
                   (version.Major < Version.Major ||
                    (version.Major == Version.Major &&
                     (version.Minor < Version.Minor ||
                      (version.Minor == Version.Minor &&
                       (version.Build == -1 || version.Build <= Version.Build)))));
        }

        bool MajorMatches(Version version)
        {
            return version.Major <= Version.Major;
        }

        bool MinorMatches(Version version)
        {
            return version.Major < Version.Major ||
                   (version.Major == Version.Major &&
                    version.Minor <= Version.Minor);
        }
    }
    public class AproximatelyGreaterVersionVertex : VersionVertex
    {
        Version _upperBound;
        Func<Version, bool> _upperBoundFunc;

        public AproximatelyGreaterVersionVertex(Version version)
            : base(version)
        {
            if (version.Build != -1)
                _upperBoundFunc = v => v < IncreaseNextToLast(version);
            else
                _upperBoundFunc = v => v.Major < (version.Major+1);
        }

        static Version IncreaseNextToLast(Version version)
        {
            var v = new Version(version.ToString().Substring(0, version.ToString().LastIndexOf(".")));

            return v.Build == -1
                       ? new Version(v.Major, v.Minor + 1)
                       : new Version(v.Major, v.Minor, v.Build + 1);
        }


        public override bool IsCompatibleWith(Version version)
        {
            return version >= Version && _upperBoundFunc(version);
        }

        public override string ToString()
        {
            return "~> " + Version.IgnoreRevision();
        }
    }
}