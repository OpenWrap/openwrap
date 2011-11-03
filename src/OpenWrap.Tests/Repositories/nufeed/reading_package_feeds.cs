using System;
using System.Collections.Generic;
using NUnit.Framework;
using OpenRasta.Client;
using OpenWrap;
using OpenWrap.Testing;
using Tests;
using Tests.Repositories.factories.nuget;

namespace Tests.Repositories.nufeed
{
    public class reading_package_feeds : contexts.nufeed
    {
        DateTimeOffset Now = DateTimeOffset.UtcNow;

        public reading_package_feeds()
        {
            given_remote_resource("http://localhost/packages/1",
                                  "application/atom+xml",
                                  AtomContent.Feed(Now, "http://localhost/packages/".ToUri(), "2".ToUri())
                                          .Entry(AtomContent.NuGetEntry("openwrap", "1.0", "summary"))
                                          .ToString());
            given_remote_resource("http://localhost/packages/2",
                                  "application/atom+xml",
                                  AtomContent.Feed(Now, "http://localhost/packages/".ToUri())
                                          .Entry(AtomContent.NuGetEntry("openfilesystem", "1.0", "summary"))
                                          .ToString());

            when_reading_packages();
        }

        [Test]
        public void all_pages_of_packages_are_read()
        {
            Packages["openfilesystem"].ShouldHaveCountOf(1);
            Packages["openwrap"].ShouldHaveCountOf(1);
        }
    }
}

namespace Tests.Repositories.contexts
{
}