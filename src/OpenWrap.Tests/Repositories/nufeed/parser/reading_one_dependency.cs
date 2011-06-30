using System.Linq;
using NUnit.Framework;
using OpenRasta.Client;
using OpenWrap.Testing;
using Tests.Repositories.contexts;
using Tests.Repositories.factories.nuget;

namespace Tests.Repositories.nufeed.parser
{
    public class reading_one_dependency : nufeed_parser
    {
        public reading_one_dependency()
        {

            given_feed(AtomContent.Feed(CreationTime, NuGetBaseUri));
            given_package(AtomContent.NuGetEntry(
                    "openwrap", 
                    "1.1.0",
                    "The best package manager for .net",
                    dependencies: "openfilesystem:1.0.0",
                    contentUri: "openwrap.nupkg"));

            when_reading_feed();
        }

        [Test]
        public void dependency_is_found()
        {
            Feed.Packages.First().Dependencies.ShouldHaveCountOf(1);
        }

        [Test]
        public void dependency_version_ignores_build()
        {
            Feed.Packages.Single().Dependencies.Single().ShouldBe("openfilesystem >= 1.0 and < 1.1");
        }

        [Test]
        public void dependency_content_is_correct()
        {
            Feed.Packages.Single().PackageHref.ShouldNotBeNull()
                .ShouldBe(NuGetBaseUri.Combine("openwrap.nupkg"));
        }
    }
}