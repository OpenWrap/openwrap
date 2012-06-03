using System;
using System.Linq;
using NUnit.Framework;
using OpenRasta.Client;
using OpenWrap.Testing;

namespace Tests.Repositories.nufeed.cached
{
    public class initial_retrieval : contexts.nufeed
    {
        DateTimeOffset Now = DateTimeOffset.UtcNow;

        public initial_retrieval()
        {
            given_remote_resource("http://localhost/packages/page/1",
                                  "application/atom+xml",
                                  AtomContent.Feed(Now, "http://localhost/packages/page/1".ToUri())
                                             .Entry(AtomContent.NuGetEntry("sauron", "1.0", "Sauron package")));

            given_repository("http://localhost/packages/page/1", cachingEnabled: true);
            when_reading_packages();
        }
        [Test]
        public void package_name_is_read()
        {
            Packages["sauron"].ShouldHaveOne();

        }
    }
}