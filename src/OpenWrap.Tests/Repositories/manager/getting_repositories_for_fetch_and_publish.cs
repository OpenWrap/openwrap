using NUnit.Framework;
using OpenWrap.Repositories;
using OpenWrap.Services;
using OpenWrap.Testing;
using Tests;
using Tests.Commands.contexts;

namespace Tests.Repositories.manager
{
    public class getting_repositories_for_fetch_and_publish : contexts.remote_manager
    {
        public getting_repositories_for_fetch_and_publish()
        {
            given_remote_factory_memory();

            given_remote_config("iron-hills", publishTokens: "[memory]somewhere");
            when_listing_repositories();
        }

        [Test]
        public void fetch_repo_built()
        {
            FetchRepositories.ShouldHaveOne()
                .Check(_ => _.Name.ShouldBe("iron-hills"))
                .Check(_ => _.Token.ShouldBe("[memory]iron-hills"));
        }
        [Test]
        public void publish_repo_built()
        {
            PublishRepositories.ShouldHaveOne()
                .Check(_ => _.Name.ShouldBe("somewhere"))
                .Check(_ => _.Token.ShouldBe("[memory]somewhere"))
                .Feature<ISupportPublishing>().ShouldNotBeNull();
        }
    }
}
