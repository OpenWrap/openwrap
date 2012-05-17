using OpenWrap;
using OpenWrap.Testing;

namespace Tests.version
{
    public static class SemanticVersionExtension
    {
        public static SemanticVersion ShouldBeBefore(this SemanticVersion left, SemanticVersion right)
        {
            left.ShouldBeLessThan(right);
            right.ShouldBeGreaterThan(left);
            return left;
        }
        public static SemanticVersion ShouldHave(
            this SemanticVersion version,
            int? major = null,
            int? minor = null,
            int? patch = null,
            string build = null,
            string pre = null
            )
        {
            if (major != null) version.Major.ShouldBe(major.Value);
            if (minor != null) version.Minor.ShouldBe(minor.Value);
            if (patch != null) version.Patch.ShouldBe(patch.Value);
            if (build != null) version.Build.ShouldBe(build);
            if (pre != null) version.PreRelease.ShouldBe(pre);
            return version;
        }
    }
}