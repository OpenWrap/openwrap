using NUnit.Framework;
using OpenWrap.Configuration.Remotes;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests.Commands.add_remote.priority
{
    public class adding_new_remote_with_same_priority_as_existing : contexts.add_remote
    {
        public adding_new_remote_with_same_priority_as_existing()
        {
            given_remote_factory(input => new InMemoryRepository(input));
            given_remote_configuration(new RemoteRepositories
            {
                { "isengard", new RemoteRepository { Name = "isengard", Priority = 666 } },
                { "fangorn", new RemoteRepository { Name = "fangorn", Priority = 667 } }
            });
            when_executing_command("iron-hills http://lotr.org/iron-hills -priority 666");
        }

        [Test]
        public void existing_remotes_are_moved()
        {
            StoredRemotesConfig["isengard"].Priority.ShouldBe(667);
            StoredRemotesConfig["fangorn"].Priority.ShouldBe(668);
        }

        [Test]
        public void priority_is_set()
        {
            StoredRemotesConfig["iron-hills"].Priority.ShouldBe(666);
        }
    }
}