using System.Linq;
using NUnit.Framework;
using OpenWrap.Testing;
using Tests.Repositories.contexts;
using Tests.Repositories.factories.nuget;

namespace Tests.Repositories.nufeed.parser
{
    public class reading_empty_dependencies : nufeed_parser
    {
        public reading_empty_dependencies()
        {

            given_feed(AtomContent.Feed(CreationTime, NuGetBaseUri));
            given_package(AtomContent.NuGetEntry("openwrap", "1.1.0", "The best package manager for .net"));

            when_reading_feed();
        }

        [Test]
        public void dependencies_are_empty()
        {
            Feed.Packages.First().Dependencies.ShouldBeEmpty();
        }
    }
}