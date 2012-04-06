using NUnit.Framework;
using OpenWrap.Testing;
using Tests.Packages.contexts;

namespace Tests.Packages.Shared
{
    [TestFixture(typeof(uncompressed_package))]
    [TestFixture(typeof(zip_package))]
    public class no_name<T> : contexts.common_package<T> where T : sut, new()
    {
        public no_name()
        {
            given_descriptor("version: 1.0.0");
            when_creating_package();
        }

        [Test]
        public void package_is_not_valid()
        {
            package.IsValid.ShouldBeFalse();
        }
    }
}