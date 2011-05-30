using System.Linq;
using NUnit.Framework;
using OpenWrap.Testing;
using Tests.Repositories.contexts;
using Tests.Repositories.factories.nuget;

namespace Tests.Repositories.nufeed.parser
{
    public class reading_multiple_dependencies_with_ranges :  nufeed_parser
    {
        public reading_multiple_dependencies_with_ranges()
        {

            given_feed(AtomContent.Feed(CreationTime, NuGetBaseUri));
            given_package(AtomContent.NuGetEntry(
                    "openwrap",
                    "1.1.0",
                    "The best package manager for .net",
                    dependencies: "openfilesystem:1.0.0|sharpziplib|mono.cecil:[0.9,1.0)"));

            when_reading_feed();
        }
        [Test]
        public void versioned_dependency_read()
        {
            Feed.Packages.Single().Dependencies.ShouldHaveOne("openfilesystem >= 1.0 and < 1.1");
        }
        [Test]
        public void unversioned_dependency_read()
        {
            Feed.Packages.Single().Dependencies.ShouldHaveOne("sharpziplib");
        }

        [Test]
        public void ranged_version_dependency_read()
        {
            Feed.Packages.Single().Dependencies.ShouldHaveOne("mono.cecil >= 0.9 and < 1.0");
        }
    }
}