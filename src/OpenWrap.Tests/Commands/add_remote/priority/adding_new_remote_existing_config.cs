using NUnit.Framework;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests.Commands.add_remote.priority
{
    public class adding_new_remote_existing_config : contexts.add_remote
    {
        public adding_new_remote_existing_config()
        {
            given_remote_config("iron-hills");
            given_remote_factory(input => new InMemoryRepository(input));

            when_executing_command("isengard http://lotr.org/isengard");
        }

        [Test]
        public void initial_priority_is_1()
        {
            ConfiguredRemotes["isengard"].Priority.ShouldBe(2);
        }
    }
}