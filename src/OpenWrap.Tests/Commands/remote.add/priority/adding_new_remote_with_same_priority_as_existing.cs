using NUnit.Framework;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests.Commands.remote.add.priority
{
    public class adding_new_remote_with_same_priority_as_existing : contexts.add_remote
    {
        public adding_new_remote_with_same_priority_as_existing()
        {
            given_remote_config("isengard", priority: 666);
            given_remote_config("fangorn", priority: 667);

            given_remote_factory_memory();
            when_executing_command("iron-hills http://lotr.org/iron-hills -priority 666");
        }

        [Test]
        public void existing_remotes_are_moved()
        {
            ConfiguredRemotes["isengard"].Priority.ShouldBe(667);
            ConfiguredRemotes["fangorn"].Priority.ShouldBe(668);
        }

        [Test]
        public void priority_is_set()
        {
            ConfiguredRemotes["iron-hills"].Priority.ShouldBe(666);
        }
    }
}