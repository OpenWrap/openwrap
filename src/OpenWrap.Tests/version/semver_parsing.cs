using NUnit.Framework;

namespace Tests.version
{
    public class semver_parsing : contexts.version
    {
        [Test]
        public void parses_build()
        {
            v("1.1.1+build.1").ShouldHave(major: 1, minor: 1, patch: 1, build: "build.1");
        }
        [Test]
        public void parses_pre()
        {
            v("1.1.1-beta.1").ShouldHave(major: 1, minor: 1, patch: 1, pre: "beta.1");
        }
        [Test]
        public void parses_build_and_pre()
        {
            v("1.1.1-beta.1+build.1").ShouldHave(
                major: 1,
                minor: 1,
                patch: 1,
                pre: "beta.1",
                build: "build.1");
        }
    }
}