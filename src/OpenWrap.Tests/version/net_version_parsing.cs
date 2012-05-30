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
        public void parses_patch_build()
        {
            v("1.1.1.1").ShouldHave(major: 1, minor: 1, patch: 1, build: "1");
        }
        [Test]
        public void parses_minor_pre()
        {
            v("1.0.0-rc").ShouldHave(major: 1, minor: 0, patch: 0, pre: "rc");
        }
        public void parses_patch_pre()
        {
            v("1.2.3.4-rc").ShouldHave(major: 1, minor: 2, patch: 3, build: "4", pre: "rc");

        }
    }
}