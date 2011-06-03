using NUnit.Framework;
using OpenWrap.Commands.Remote.Messages;
using OpenWrap.Configuration.Remotes;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests.Commands.add_remote
{
    class adding_publish_to_existing_with_readonly_repo : contexts.add_remote
    {
        public adding_publish_to_existing_with_readonly_repo()
        {
            given_remote_configuration(new RemoteRepositories { { "iron-hills", new RemoteRepository { FetchRepository = new RemoteRepositoryEndpoint { Token = "[indexed]unknown" } } } });
            given_remote_factory(input => new InMemoryRepository(input) { CanPublish = false });

            when_executing_command("iron-hills -publish somewhere");
        }

        [Test]
        public void error_is_returned()
        {
            Results.ShouldHaveOne<RemoteEndpointReadOnly>()
                .Path.ShouldBe("somewhere");
        }

        [Test]
        public void publish_not_modified()
        {
            StoredRemotesConfig["iron-hills"].PublishRepositories.ShouldBeEmpty();
        }
    }
}