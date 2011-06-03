using System.Linq;
using NUnit.Framework;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests.Commands.add_remote
{
    public class add_publish_to_existing_with_authentication : contexts.add_remote
    {
        public add_publish_to_existing_with_authentication()
        {
            given_remote_config("iron-hills");
            given_remote_factory(userInput => new InMemoryRepository(userInput));
            when_executing_command("iron-hills -publish http://sauron -username forlong.the.fat -password lossarnach");
        }

        [Test]
        public void password_is_persisted()
        {
            ConfiguredRemotes["iron-hills"].PublishRepositories.First().Password.ShouldBe("lossarnach");
        }

        [Test]
        public void username_is_persisted()
        {
            ConfiguredRemotes["iron-hills"].PublishRepositories.First().Username.ShouldBe("forlong.the.fat");
        }
    }
}