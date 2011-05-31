using System.Linq;
using NUnit.Framework;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests.Commands.add_remote
{
    class adding_remote_by_name_only : contexts.add_remote
    {
        public adding_remote_by_name_only()
        {
            given_remote_factory(input => new InMemoryRepository(input));

            when_executing_command("iron-hills");
        }

        [Test]
        public void remote_fetch_is_added()
        {
            Remotes["iron-hills"].FetchRepository.ShouldBe("[memory]iron-hills");
        }

        [Test]
        public void remote_publish_is_added()
        {
            Remotes["iron-hills"].PublishRepositories.Single().ShouldBe("[memory]iron-hills");
        }
    }
}