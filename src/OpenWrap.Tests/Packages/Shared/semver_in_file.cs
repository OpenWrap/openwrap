using NUnit.Framework;
using OpenWrap;
using OpenWrap.Testing;
using Tests.Packages.contexts;

namespace Tests.Packages.Shared
{
    [TestFixture(typeof(uncompressed_package))]
    [TestFixture(typeof(zip_package))]
    public class semver_in_file<T> : contexts.common_package<T> where T : sut, new()
    {
        public semver_in_file()
        {
            given_descriptor("name: one-ring");
            given_file("version", "1.0.0");
            when_creating_package();
        }

        [Test]
        public void version_is_set()
        {
            package.Version.ShouldBe("1.0.0".ToVersion());
        }

        [Test]
        public void semver_is_set()
        {
            package.SemanticVersion.ShouldBe("1.0.0");
        }
    }
}