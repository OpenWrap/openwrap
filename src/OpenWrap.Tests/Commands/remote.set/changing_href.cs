using System.Linq;
using NUnit.Framework;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests.Commands.remote.set
{
    public class changing_href : contexts.set_remote
    {
        public changing_href()
        {
            given_remote_config("secundus");
            given_remote_factory(input => new InMemoryRepository(input));
            when_executing_command("secundus -href http://awesomereps.net");
        }

        [Test]
        public void repository_has_new_fetch()
        {
            ConfiguredRemotes["secundus"].FetchRepository.Token
                .ShouldBe("[memory]http://awesomereps.net");
        }

        [Test]
        public void repository_has_new_publish()
        {
            ConfiguredRemotes["secundus"].PublishRepositories.Single().Token
                .ShouldBe("[memory]http://awesomereps.net");
        }
    }
}