using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Testing;
using Tests;
using Tests.Repositories.contexts;
using Tests.Repositories.factories.nuget;

namespace Tests.Repositories.nufeed.parser
{
    public class reading_feed : nufeed_parser
    {
        public reading_feed()
        {
            given_feed(AtomContent.Feed(CreationTime, NuGetBaseUri));
            given_package(AtomContent.NuGetEntry("openwrap", "1.1.0", "The best package manager for .net"));

            when_reading_feed();
        }

        [Test]
        public void name_is_correct()
        {
            Feed.Packages.First().Name.ShouldBe("openwrap");
        }

        [Test]
        public void version_is_correct()
        {
            Feed.Packages.First().Version.ShouldBe("1.1.0");
        }

        [Test]
        public void description_is_correct()
        {
            Feed.Packages.First().Description.ShouldBe("The best package manager for .net");
        }
    }
}