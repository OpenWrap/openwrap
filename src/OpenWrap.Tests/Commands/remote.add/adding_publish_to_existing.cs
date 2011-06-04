using System.Linq;
using NUnit.Framework;
using OpenWrap.Commands.Remote.Messages;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests.Commands.remote.add
{
    class adding_publish_to_existing : contexts.add_remote
    {
        public adding_publish_to_existing()
        {
            given_remote_config("iron-hills");
            given_remote_factory(input => new InMemoryRepository(input));

            when_executing_command("iron-hills -publish somewhere");
        }

        [Test]
        public void comand_succeeds()
        {
            Results.ShouldHaveNoError();
            Results.ShouldHaveOne<RemotePublishEndpointAdded>()
                .Check(x => x.Name.ShouldBe("iron-hills"))
                .Check(x => x.Path.ShouldBe("somewhere"));
        }

        [Test]
        public void publish_is_added()
        {
            ConfiguredRemotes["iron-hills"].PublishRepositories.Single().Token.ShouldBe("[memory]somewhere");
        }
    }
}