using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Testing;

namespace Tests.Repositories
{
    public class converting_package : contexts.nuget_converter
    {
        public converting_package()
        {
            given_nuget_package(TestFiles.TestPackageOld);
            when_converting_package();
        }
        [Test]
        public void name_is_correct()
        {
            Package.Name.ShouldBe("TestPackage");
        }
        [Test]
        public void version_is_correct()
        {
            Package.Version.ShouldBe("1.0.0.1234".ToVersion());
        }
        [Test]
        public void exact_version_dependency_is_per_latest()
        {
            Package.Dependencies.First(x => x.Name == "one-ring").ToString().ShouldBe("one-ring >= 1.0 and < 1.1");
        }
        [Test]
        public void min_version_dependency_is_correct()
        {
            Package.Dependencies.First(x => x.Name == "shire").ToString().ShouldBe("shire >= 2.0.0");
        }
        [Test]
        public void assembly_is_in_bin_folder()
        {
            var package = Package.Load();
            package.Content.Single(x=>x.Key == "bin-net20").ShouldHaveCountOf(1)
                .First().File.Name.ShouldBe("empty.dll");
        }
    }
}