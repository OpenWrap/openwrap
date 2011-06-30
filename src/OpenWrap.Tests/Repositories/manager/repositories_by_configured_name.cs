using System;
using System.Linq;
using NUnit.Framework;
using OpenWrap.Testing;
using Tests.Repositories.contexts;

namespace Tests.Repositories.manager
{
    class repositories_by_configured_name : remote_manager
    {
        public repositories_by_configured_name()
        {
            given_remote_repository("iron-hills", priority: 10);
            given_remote_repository("iron-hills2", priority: 20);
            when_listing_repositories("iron-hills2");
        }

        [Test]
        public void fetches_from_named_are_returned_before_higher_priority_ones()
        {
            FetchRepositories.ShouldHaveCountOf(2)
                .Check(x => x.ElementAt(0).Name.ShouldBe("iron-hills2"))
                .Check(x => x.ElementAt(1).Name.ShouldBe("iron-hills"));
        }
    }
}