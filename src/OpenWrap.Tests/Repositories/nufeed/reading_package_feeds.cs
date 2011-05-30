using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenRasta.Client;
using OpenWrap;
using OpenWrap.PackageModel;
using OpenWrap.Repositories.NuFeed;
using OpenWrap.Testing;
using Tests;
using Tests.contexts;
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
    public class nufeed : http
    {
        protected ILookup<string, IPackageInfo> Packages;

        protected void when_reading_packages()
        {
            Packages = new NuFeedRepository(base.Client, "http://localhost/packages/1".ToUri(), "http://localhost/packages/1".ToUri())
                    .PackagesByName;
        }
    }
}