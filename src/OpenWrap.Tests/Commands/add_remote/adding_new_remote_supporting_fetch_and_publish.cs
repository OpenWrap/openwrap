using System.Linq;
using NUnit.Framework;
using OpenWrap.Commands.Remote;
using OpenWrap.Configuration;
using OpenWrap.Repositories;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace Tests.Commands.add_remote
{
    public class adding_new_remote_supporting_fetch_and_publish : remote_command<AddRemoteCommand>
    {
        public adding_new_remote_supporting_fetch_and_publish()
        {
            given_remote_factory(input => new InMemoryRepository(input));
            given_remote_configuration(new RemoteRepositories());
            when_executing_command("iron-hills http://lotr.org/iron-hills");
        }

        [Test]
        public void remote_is_added_to_the_list()
        {
            ConfiguredRemotes.ContainsKey("iron-hills").ShouldBeTrue();
        }
        [Test]
        public void remote_has_priority_of_one()
        {
            ConfiguredRemotes["iron-hills"].Priority.ShouldBe(1);
        }

        [Test]
        public void remote_has_correct_fetch_token()
        {
            ConfiguredRemotes["iron-hills"].FetchRepository.ShouldBe("[memory]http://lotr.org/iron-hills");
        }

        [Test]
        public void remote_has_correct_publish_token()
        {
            ConfiguredRemotes["iron-hills"].PublishRepositories.ShouldHaveCountOf(1).First().ShouldBe("[memory]http://lotr.org/iron-hills");
        }
    }
}