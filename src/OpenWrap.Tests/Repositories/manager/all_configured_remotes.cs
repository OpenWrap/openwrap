using System.Linq;
using NUnit.Framework;
using OpenWrap.Repositories;
using OpenWrap.Testing;
using Tests.Repositories.contexts;

namespace Tests.Repositories.manager
{
    public class all_configured_remotes : remote_manager
    {
        public all_configured_remotes()
        {
            given_remote_factory_memory();

            given_remote_config("iron-hills2", priority: 2);
            given_remote_config("iron-hills", priority: 1, publishTokens: "[memory]somewhere");
            when_listing_repositories();
        }

        [Test]
        public void fetch_order_is_preserved()
        {
            FetchRepositories.ShouldHaveCountOf(2)
                .Check(_ => _.ElementAt(0).Name.ShouldBe("iron-hills"))
                .Check(_ => _.ElementAt(1).Name.ShouldBe("iron-hills2"));
        }

        [Test]
        public void fetch_repo_built()
        {
            FetchRepositories.ShouldHaveAtLeastOne().First()
                .Check(_ => _.Name.ShouldBe("iron-hills"))
                .Check(_ => _.Token.ShouldBe("[memory]iron-hills"));
        }

        [Test]
        public void publish_repo_built()
        {
            PublishRepositories.ShouldHaveAtLeastOne().First()
                .Check(_ => _.Name.ShouldBe("iron-hills - somewhere"))
                .Check(_ => _.Token.ShouldBe("[memory]somewhere"))
                .Feature<ISupportPublishing>().ShouldNotBeNull();
        }
    }
}