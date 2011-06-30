using System;
using System.Linq;
using NUnit.Framework;
using OpenRasta.Client;
using OpenWrap.Testing;
using Tests.Repositories.contexts;
using Tests.Repositories.factories.nuget;

namespace Tests.Repositories.nufeed.parser
{
    public class reading_links : nufeed_parser
    {
        public reading_links()
        {
            given_feed(
                    AtomContent.Feed(DateTimeOffset.Now, "http://localhost/".ToUri(), "http://localhost/next".ToUri()));
            when_reading_feed();
        }

        [Test]
        public void next_link_is_found()
        {
            Feed.Links["next"].First().Href.ShouldBe("http://localhost/next");
        }
    }
}