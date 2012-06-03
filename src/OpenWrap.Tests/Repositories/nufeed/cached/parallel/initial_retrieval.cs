using System;
using System.Xml.Linq;
using NUnit.Framework;
using OpenRasta.Client;
using OpenWrap.Testing;

namespace Tests.Repositories.nufeed.cached.parallel
{
    public class initial_retrieval : contexts.nufeed
    {
        DateTimeOffset Now = DateTimeOffset.UtcNow;
        public initial_retrieval()
        {
            given_not_found_response(_=>true, AtomContent.Feed(Now));
            given_remote_resource(
                "http://localhost/packages?$filter=startswith(Id,'a')",
                "application/atom+xml",
                AtomContent.Feed(
                    Now,
                    "http://loclahost/packages".ToUri()
                ).Entry(
                    AtomContent.NuGetEntry("aragorn", "1.0.0", "aragorn")
                )
            );

            given_remote_resource(
                "http://localhost/packages?$filter=startswith(Id,'b')",
                "application/atom+xml",
                AtomContent.Feed(
                    Now,
                    "http://loclahost/packages".ToUri()
                ).Entry(
                    AtomContent.NuGetEntry("boromir", "1.0.0", "boromir")
                )
            );

            given_repository("http://localhost/packages", cachingEnabled: true);
            when_reading_packages();
        }

        [Test]
        public void first_letter_is_read()
        {
            Packages["aragorn"].ShouldHaveOne();
        }
        [Test]
        public void second_letter_is_read()
        {
            Packages["boromir"].ShouldHaveOne();
        }
    }
}