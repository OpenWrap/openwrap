using System.Linq;
using NUnit.Framework;
using Tests.Repositories.contexts;

namespace Tests.Repositories.nufeed.parser
{
    public class nuget_pre_version : nufeed_parser
    {
        public nuget_pre_version()
        {
            given_feed(AtomContent.Feed(CreationTime, NuGetBaseUri));
            given_package(AtomContent.NuGetEntry("nuget", "1.0.0.0-beta", "summary"));

            when_reading_feed();
        }
        [Test]
        public void package_is_read()
        {
            Feed.Packages.First().Version.ShouldBe("1.0.0-beta+0");
        }
    }
}