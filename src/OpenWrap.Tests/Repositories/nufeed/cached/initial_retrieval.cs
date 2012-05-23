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

            given_repository("http://localhost/packages/page/1");
            when_reading_packages();
        }
        [Test]
        public void package_name_is_read()
        {
            Packages["sauron"].ShouldHaveOne();

        }
    }
    public class updates_available : contexts.nufeed
    {
        DateTimeOffset Now;
        DateTimeOffset Later;

        public updates_available()
        {
            Now = new DateTimeOffset();
            Later = Now + 2.Minutes();

            given_remote_resource("http://localhost/packages",
                                  "application/atom+xml",
                                  AtomContent.Feed(Now, "http://localhost/packages/page/1".ToUri())
                                             .Entry(AtomContent.NuGetEntry("sauron", "1.0", "Sauron package")));
            given_remote_resource(
                string.Format("http://localhost/packages?LastUpdated gt datetime'{0}'", Now),
                "application/atom+xml",
                AtomContent.Feed(Later, "http://locahost/packages".ToUri())
                           .Entry(AtomContent.NuGetEntry("one-ring", "1.0", "One to rule them all"))
                );
            given_repository("http://localhost/packages", cachingEnabled: true);
            given_packages_read_once();
            when_updating_cache();
        }

        [Test]
        public void previous_oackages_are_read()
        {
            Packages["sauron"].ShouldHaveOne();
        }
        [Test]
        public void new_packages_are_read()
        {
            Packages["one-ring"].ShouldHaveOne();
        }
    }
}