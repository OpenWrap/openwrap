using NUnit.Framework;
using OpenWrap.Configuration.Remotes;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests.Commands.add_remote.priority
{
    public class adding_new_remote_existing_config : contexts.add_remote
    {
        public adding_new_remote_existing_config()
        {
            given_remote_factory(input => new InMemoryRepository(input));
            given_remote_configuration(new RemoteRepositories { { "iron-hills", new RemoteRepository { Name = "iron-hills", Priority = 1 } } });
            when_executing_command("isengard http://lotr.org/isengard");
        }

        [Test]
        public void initial_priority_is_1()
        {
            StoredRemotesConfig["isengard"].Priority.ShouldBe(2);
        }
    }
}