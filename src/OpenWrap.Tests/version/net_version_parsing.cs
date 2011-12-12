using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.version
{
    public class net_version_parsing : contexts.version
    {
        [Test]
        public void null_returns_null()
        {
            v(null).ShouldBeNull();
        }
        [Test]
        public void parses_major()
        {
            v("1").ShouldHave(major: 1);
        }
        [Test]
        public void parses_minor()
        {
            v("1.1").ShouldHave(major: 1, minor: 1);
        }
        [Test]
        public void parses_patch()
        {
            v("1.1.1").ShouldHave(major: 1, minor: 1, patch: 1);
        }
        [Test]
        public void parses_patchr_build()
        {
            v("1.1.1.1").ShouldHave(major: 1, minor: 1, patch: 1, build: "1");
        }
    }
}