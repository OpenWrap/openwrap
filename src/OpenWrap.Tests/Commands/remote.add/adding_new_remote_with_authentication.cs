using System.Linq;
using NUnit.Framework;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests.Commands.remote.add
{
    public class adding_new_remote_with_authentication : contexts.add_remote
    {
        public adding_new_remote_with_authentication()
        {
            given_remote_factory(userInput => new InMemoryRepository(userInput));
            when_executing_command("iron-hills http://sauron -username forlong.the.fat -password lossarnach");
        }

        [Test]
        public void password_is_persisted()
        {
            ConfiguredRemotes["iron-hills"].FetchRepository.Password.ShouldBe("lossarnach");
            ConfiguredRemotes["iron-hills"].PublishRepositories.First().Password.ShouldBe("lossarnach");
        }

        [Test]
        public void username_is_persisted()
        {
            ConfiguredRemotes["iron-hills"].FetchRepository.Username.ShouldBe("forlong.the.fat");
            ConfiguredRemotes["iron-hills"].PublishRepositories.First().Username.ShouldBe("forlong.the.fat");
        }
    }
}