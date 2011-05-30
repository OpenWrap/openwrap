using System.Linq;
using NUnit.Framework;
using OpenWrap.Testing;
using Tests.Repositories.contexts;
using Tests.Repositories.factories.nuget;

namespace Tests.Repositories.nufeed.parser
{
    public class reading_one_dependency_without_version : nufeed_parser
    {
        public reading_one_dependency_without_version()
        {

            given_feed(AtomContent.Feed(CreationTime, NuGetBaseUri));
            given_package(AtomContent.NuGetEntry(
                    "openwrap",
                    "1.1.0",
                    "The best package manager for .net",
                    dependencies: "openfilesystem"));

            when_reading_feed();
        }
        [Test]
        public void dependency_version_ignores_build()
        {
            Feed.Packages.Single().Dependencies.Single().ShouldBe("openfilesystem");
        }
    }
}