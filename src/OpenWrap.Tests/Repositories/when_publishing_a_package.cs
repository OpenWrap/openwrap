using System;
using System.Linq;
using NUnit.Framework;
using OpenWrap.IO;
using OpenWrap.Testing;

namespace Tests.Repositories
{
    public class when_publishing_a_package : context.indexed_folder_repository
    {
        public when_publishing_a_package()
        {
            given_file_system(@"c:\tmp");
            given_indexed_repository(@"c:\tmp\repository");

            when_publishing_package(Package("isengard", "2.1", "depends: saruman"));
        }

        [Test]
        public void index_file_exists()
        {
            Repository.IndexFeed.ShouldNotBeNull();
        }
        [Test]
        public void index_file_is_not_empty()
        {
            IndexDocument.Document.ShouldNotBeNull();
        }
        [Test]
        public void index_file_contains_package()
        {
            var package = IndexDocument.Document.Descendants("wrap").FirstOrDefault();
            package.ShouldNotBeNull();
            package.Attribute("name").ShouldNotBeNull().Value.ShouldBe("isengard");
            package.Attribute("version").ShouldNotBeNull().Value.ShouldBe("2.1");
            package.Attribute("semantic-version").ShouldNotBeNull().Value.ShouldBe("2.1");
            var link = package.Descendants("link").FirstOrDefault().ShouldNotBeNull();
            link.Attribute("href").ShouldNotBeNull().Value.ShouldNotBeNull().ShouldContain("isengard-2.1.wrap");
        }
        [Test]
        public void index_file_contains_package_dependencies()
        {
            Repository.PackagesByName["isengard"].First()
                    .Dependencies.ShouldHaveCountOf(1)
                    .First().Name.ShouldBe("saruman");
        }
        [Test]
        public void package_is_accessible()
        {
            Repository.PackagesByName["isengard"].FirstOrDefault().ShouldNotBeNull()
                .Load().ShouldNotBeNull()
                .OpenStream().ReadToEnd().Length.ShouldBeGreaterThan(0);
        }
    }
}
