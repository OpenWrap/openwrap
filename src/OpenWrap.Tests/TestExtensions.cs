using System.Linq;
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
                .Version.ShouldBe(version.ToVersion());
        }
        public static void ShouldNotHaveLock(this ISupportLocking repo, string name, string scope = null)
        {
            repo.LockedPackages[scope ?? string.Empty].SingleOrDefault(x => x.Name == name).ShouldBeNull();
        }
    }
}