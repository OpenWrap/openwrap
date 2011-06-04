using System.Linq;
using NUnit.Framework;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests.Commands.remote.set
{
    public class changing_fetch_and_publish : contexts.set_remote
    {
        public changing_fetch_and_publish()
        {
            given_remote_config("primus", publishTokens: "[memory]http://evil.land.com");
            given_remote_factory(input => new InMemoryRepository(input));
            when_executing_command("primus -href http://openwrap.org -publish http://openwrap2.org");
        }

        [Test]
        public void fetch_is_changed()
        {
            ConfiguredRemotes["primus"].FetchRepository.Token
                .ShouldBe("[memory]http://openwrap.org");
        }

        [Test]
        public void publish_is_changed()
        {
            ConfiguredRemotes["primus"].PublishRepositories.Single().Token
                .ShouldBe("[memory]http://openwrap2.org");
        }
    }
}