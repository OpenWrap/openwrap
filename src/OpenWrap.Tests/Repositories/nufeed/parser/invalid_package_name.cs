using NUnit.Framework;
using OpenWrap.Testing;
using Tests.Repositories.contexts;

namespace Tests.Repositories.nufeed.parser
{
    public class invalid_package_name : nufeed_parser
    {
        public invalid_package_name()
        {
            given_feed(AtomContent.Feed(CreationTime, NuGetBaseUri));
            given_package(AtomContent.NuGetEntry("*", "1.0.0", "summary"));

            when_reading_feed();
        }
        [Test]
        public void package_is_ignored()
        {
            Feed.Packages.ShouldBeEmpty();
        }
    }
}