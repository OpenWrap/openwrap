using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.Repositories
{
    public class convertings_package_from_non_seekable_stream : contexts.nuget_converter
    {
        public convertings_package_from_non_seekable_stream()
        {
            given_readonly_nu_package(TestFiles.TestPackageOld);
            when_converting_package();
        }
        [Test]
        public void package_is_converted()
        {
            Package.ShouldNotBeNull();
        }
    }
}