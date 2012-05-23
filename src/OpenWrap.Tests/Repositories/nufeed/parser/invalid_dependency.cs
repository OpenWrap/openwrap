using NUnit.Framework;
using OpenWrap.Testing;
using Tests.Repositories.contexts;

namespace Tests.Repositories.nufeed.parser
{
    public class invalid_dependency : nufeed_parser
    {
        public invalid_dependency()
        {
            given_feed(AtomContent.Feed(CreationTime, NuGetBaseUri));
            given_package(AtomContent.NuGetEntry("nuget", "1.0.0", "summary",
                                                 dependencies: "(something):[1.0]"));

            when_reading_feed();
        }
        [Test]
        public void package_is_ignored()
        {
            Feed.Packages.ShouldBeEmpty();
        }
    }
}