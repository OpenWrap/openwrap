using System.Linq;
using System.Text.RegularExpressions;
using OpenWrap;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests
{
    public static class TestExtensions
    {
        public static void ShouldHaveLock(this ISupportLocking repo, string name, string version, string scope = null)
        {
            repo.LockedPackages[scope ?? string.Empty].SingleOrDefault(x => x.Name == name).ShouldNotBeNull()
                .SemanticVersion.ShouldBe(version);
        }
        public static void ShouldNotHaveLock(this ISupportLocking repo, string name, string scope = null)
        {
            repo.LockedPackages[scope ?? string.Empty].SingleOrDefault(x => x.Name == name).ShouldBeNull();
        }
        public static SemanticVersion ShouldBe(this SemanticVersion left, string right)
        {
            left.ShouldBe(SemanticVersion.TryParseExact(right));
            return left;
        }
        public static string ShouldMatch(this string source, string regex)
        {
            Regex.IsMatch(source, regex).ShouldBeTrue();
            return source;
        }
    }
}