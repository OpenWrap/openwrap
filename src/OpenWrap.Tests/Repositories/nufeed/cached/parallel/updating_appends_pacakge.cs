using System;
using NUnit.Framework;
using OpenRasta.Client;
using OpenWrap.Testing;

namespace Tests.Repositories.nufeed.cached.parallel
{
    public class updating_appends_pacakge : contexts.nufeed
    {
        public updating_appends_pacakge()
        {
            var now = DateTimeOffset.UtcNow;
            var later = now + 1.Minutes();

            given_not_found_response(_=>_.RequestUri.AbsolutePath == "/packages", AtomContent.Feed(now));
            
            given_remote_resource(
                "http://localhost/packages?$filter=startswith(Id,'a')",
                "application/atom+xml",
                AtomContent.Feed(
                    now,
                    "http://loclahost/packages".ToUri()
                    ).Entry(
                        AtomContent.NuGetEntry("aragorn", "1.0.0", "aragorn")
                    )
                );

            given_remote_resource(
                string.Format("http://localhost/packages?LastUpdated gt datetime'{0:yyyy-MM-ddThh:mm:ss}'&$filter=startswith(Id,'a')", now),
                "application/atom+xml",
                AtomContent.Feed(
                    later,
                    "http://loclahost/packages".ToUri()
                    ).Entry(
                        AtomContent.NuGetEntry("boromir", "1.0.0", "boromir")
                    )
                );
            given_repository("http://localhost/packages", cachingEnabled: true);
            given_packages_read_once();
            when_updating_cache();
        }
        [Test]
        public void package_is_appended()
        {
            Packages["boromir"].ShouldHaveOne();
        }
    }
}