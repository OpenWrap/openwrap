using NUnit.Framework;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests.Commands.set_remote
{
    public class changing_href_to_readonly : contexts.set_remote
    {
        public changing_href_to_readonly()
        {
            given_remote_config("secundus");
            given_remote_factory(input => new InMemoryRepository(input) { CanPublish = false });
            when_executing_command("secundus -href http://awesomereps.net");
        }

        [Test]
        public void repository_has_new_fetch()
        {
            ConfiguredRemotes["secundus"].FetchRepository.Token
                .ShouldBe("[memory]http://awesomereps.net");
        }

        [Test]
        public void repository_has_no_publish()
        {
            ConfiguredRemotes["secundus"].PublishRepositories.ShouldBeEmpty();
        }
    }
}