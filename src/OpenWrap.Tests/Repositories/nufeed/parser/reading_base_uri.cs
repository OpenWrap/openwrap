using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenRasta.Client;
using OpenWrap;
using OpenWrap.Testing;
using Tests;
using Tests.Repositories.factories.nuget;

namespace Tests.Repositories.nufeed.parser
{
    public class reading_base_uri : contexts.nufeed_parser
    {
        public reading_base_uri()
        {
            given_feed(AtomContent.Feed(DateTimeOffset.Now, "http://localhost/base".ToUri()));
            when_reading_feed();
        }

        [Test]
        public void base_uri_is_read()
        {
            Feed.BaseUri.ShouldBe("http://localhost/base");
        }
    }
}