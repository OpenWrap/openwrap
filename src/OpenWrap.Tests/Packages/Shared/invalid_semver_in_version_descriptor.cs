using NUnit.Framework;
using OpenWrap.Testing;
using Tests.Packages.contexts;

namespace Tests.Packages.Shared
{
    [TestFixture(typeof(uncompressed_package))]
    [TestFixture(typeof(zip_package))]
    public class invalid_semver_in_version_descriptor<T> : contexts.common_package<T> where T : sut, new()
    {
        public invalid_semver_in_version_descriptor()
        {
            given_descriptor("name: one-ring", "version: 1.0.0+1");
            when_creating_package();
        }

        [Test]
        public void package_is_not_valid()
        {
            package.IsValid.ShouldBeFalse();
        }
    }
}