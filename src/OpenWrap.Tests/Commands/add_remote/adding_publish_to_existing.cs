using System.Linq;
using NUnit.Framework;
using OpenWrap.Configuration;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests.Commands.add_remote
{
    class adding_publish_to_existing : contexts.add_remote
    {
        public adding_publish_to_existing()
        {
            given_remote_configuration(new RemoteRepositories { { "iron-hills", new RemoteRepository { FetchRepository = "[indexed]unknown" } } });
            given_remote_factory(input => new InMemoryRepository(input));

            when_executing_command("iron-hills -publish somewhere");
        }

        [Test]
        public void comand_succeeds()
        {
            Results.ShouldHaveNoError();
        }
        [Test]
        public void publish_is_added()
        {
            Remotes["iron-hills"].PublishRepositories.Single().ShouldBe("[memory]somewhere");
        }
    }
}