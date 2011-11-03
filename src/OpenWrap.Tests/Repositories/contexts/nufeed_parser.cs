using System;
using System.Xml.Linq;
using OpenRasta.Client;
using OpenWrap.Repositories.Http;
using OpenWrap.Repositories.NuFeed;
using OpenWrap.Testing;

namespace Tests.Repositories.contexts
{
    public abstract class nufeed_parser : OpenWrap.Testing.context
    {
        XDocument _xFeed;
        protected PackageFeed Feed;
        protected DateTimeOffset CreationTime = new DateTimeOffset(2009, 10, 24, 0, 0, 0, TimeSpan.Zero);
        protected Uri NuGetBaseUri = "http://packages.nuget.org/v1/FeedService.svc/".ToUri();


        protected void when_reading_feed()
        {
            Feed = NuFeedReader.Read(_xFeed.CreateReader());
        }

        protected void given_package(XElement package)
        {
            _xFeed.Feed().Add(package);
        }

        protected void given_feed(XDocument feed)
        {
            _xFeed = feed;
        }
    }
}