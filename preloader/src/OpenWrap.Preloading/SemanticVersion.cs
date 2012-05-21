using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenWrap
{
    public class SemanticVersion : IComparable<SemanticVersion>, IEquatable<SemanticVersion>
    {
        public SemanticVersion(int major, int minor = -1, int patch = -1, string pre = null, string build = null)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
            PreRelease = pre;
            Build = build;
        }

        static Regex _netver = new Regex(@"^" + NETVER_REGEX + @"$");
        static Regex _semver = new Regex(
                @"^" + 
                SEMVER_REGEX +
                @"$");

        const string NETVER_REGEX = @"(?<major>\d+)(\.(?<minor>\d+)(\.(?<patch>\d+)" +
            @"(\.(?<build>\d+))?)?)?" + @"(\-(?<pre>[0-9A-Za-z-\.]+))?";

        const string SEMVER_REGEX = @"(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)" +
                @"(\-(?<pre>[0-9A-Za-z-\.]+))?" +
                @"(\+(?<build>[0-9A-Za-z-\.]+))?";

        public int Major { get; private set; }

        public int Minor { get; private set; }

        public int Patch { get; private set; }

        public string Build { get; private set; }

        public string PreRelease { get; private set; }

        public static SemanticVersion TryParseExact(string text)
        {
            if (text == null) return null;
            var match = _semver.Match(text);
            if (!match.Success)
                match = _netver.Match(text);
            if (!match.Success)
                return null;

            return new SemanticVersion(
                GetInt(match, "major"), 
                GetInt(match, "minor"), 
                GetInt(match, "patch"), 
                GetString(match, "pre"), 
                GetString(match, "build")
                );
        }
        
        public bool Equals(SemanticVersion other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return NumericEqual(other.Major, Major) &&
                NumericEqual(other.Minor, Minor) &&
                NumericEqual(other.Patch, Patch) &&
                Equals(other.Build, Build) &&
                Equals(other.PreRelease, PreRelease);
        }
        bool NumericEqual(int first, int second)
        {
            first = first == -1 ? 0 : first;
            second = second == -1 ? 0 : second;
            return first == second;
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(SemanticVersion)) return false;
            return Equals((SemanticVersion)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = Major;
                result = (result * 397) ^ Minor;
                result = (result * 397) ^ Patch;
                result = (result * 397) ^ (Build != null ? Build.GetHashCode() : 0);
                result = (result * 397) ^ (PreRelease != null ? PreRelease.GetHashCode() : 0);
                return result;
            }
        }

        public static bool operator ==(SemanticVersion left, SemanticVersion right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SemanticVersion left, SemanticVersion right)
        {
            return !Equals(left, right);
        }

        public static bool operator >(SemanticVersion left, SemanticVersion right)
        {
            return right < left;
        }
        public static bool operator <(SemanticVersion left, SemanticVersion right)
        {
            if (left == null) throw new ArgumentNullException("left");
            return left.CompareTo(right) < 0;
        }

        public static bool operator >=(SemanticVersion left, SemanticVersion right)
        {
            return right <= left;
        }
        public static bool operator <=(SemanticVersion left, SemanticVersion right)
        {
            if (left == null) throw new ArgumentNullException("left");
            return left.CompareTo(right) <= 0;
        }
        static string GetString(Match match, string name)
        {
            return match.Groups[name].Success ? match.Groups[name].Value : null;
        }
        static int GetInt(Match match, string name)
        {
            return match.Groups[name].Success ? int.Parse(match.Groups[name].Value) : -1;
        }

        public int CompareTo(SemanticVersion other)
        {
            if (other == null) return -1;
            if (Major != other.Major) return Major - other.Major;
            if (Minor != other.Minor) return Minor - other.Minor;
            if (Patch != other.Patch) return Patch - other.Patch;

            if (PreRelease != other.PreRelease)
            {
                if (PreRelease == null) return 1;
                if (other.PreRelease == null) return -1;
                return CompareSegments(PreRelease, other.PreRelease);
            }
            if (Build != other.Build)
            {
                if (Build == null) return -1;
                if (other.Build == null) return 1;
                return CompareSegments(Build, other.Build);
            }
            return 0;
        }

        int CompareSegments(string left, string right)
        {
            var leftSegs = left.Split('.');
            var rightSegs = right.Split('.');
            int i;
            for (i = 0; i < leftSegs.Length; i++)
            {
                if (rightSegs.Length < i + 1) return 1;
                if (leftSegs[i] == rightSegs[i]) continue;
                //if (rightSegs.Length < i) return missingHasPriority ? 1 : -1;
                int leftNumeric, rightNumeric;
                if (!int.TryParse(leftSegs[i], out leftNumeric)) leftNumeric = -1;
                if (!int.TryParse(rightSegs[i], out rightNumeric)) rightNumeric = -1;

                if (leftNumeric != -1 && rightNumeric != -1)
                    return leftNumeric.CompareTo(rightNumeric);
                if (leftNumeric == -1 && rightNumeric == -1)
                    return leftSegs[i].CompareTo(rightSegs[i]);

                return rightNumeric;

            }
            if (rightSegs.Length >= i) return -1;
            return 0;
        }
        public override string ToString()
        {
            var sb = new StringBuilder().Append(Major);
            if (Minor >= 0) sb.Append(".").Append(Minor);
            if (Patch >= 0) sb.Append(".").Append(Patch);
            if (PreRelease != null) sb.Append("-").Append(PreRelease);
            if (Build != null) sb.Append("+").Append(Build);
            return sb.ToString();
        }

        public Version ToVersion()
        {
            int build;
            if (!int.TryParse(Build, out build)) build = -1;
            if (Patch != -1)
                if (build != -1)
                    return new Version(Major, Minor, Patch, build);
                else
                    return new Version(Major, Minor, Patch);
            return new Version(Major, Minor);
        }
    }
}
