using System.Linq;
using NUnit.Framework;
using OpenWrap.Repositories;
using OpenWrap.Testing;
using Tests.Repositories.contexts;

namespace Tests.Repositories.manager
{
    class repositories_by_user_input : remote_manager
    {
        public repositories_by_user_input()
        {
            given_remote_repository("iron-hills", priority: 10);
            given_remote_factory_additional((userInput,cred) => userInput == "celduin" ? new InMemoryRepository("celduin") : null);
            when_listing_repositories("celduin");
        }

        [Test]
        public void fetches_from_named_are_returned_before_higher_priority_ones()
        {
            FetchRepositories.ShouldHaveCountOf(2)
                .Check(x => x.ElementAt(0).Name.ShouldBe("celduin"))
                .Check(x => x.ElementAt(1).Name.ShouldBe("iron-hills"));
        }

        [Test]
        public void publish_from_named_are_returned_before_higher_priority_ones()
        {
            PublishRepositories.ShouldHaveCountOf(2)
                .Check(x => x.ElementAt(0).Name.ShouldBe("celduin"))
                .Check(x => x.ElementAt(1).Name.ShouldBe("iron-hills"));
        }
    }
}