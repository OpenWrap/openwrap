using System;
using System.Linq;
using NUnit.Framework;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests.Commands.set_remote
{
    public class changing_publish : contexts.set_remote
    {
        public changing_publish()
        {
            given_remote_config("primus");
            given_remote_factory(input => new InMemoryRepository(input));
            when_executing_command("primus -publish openwrap");
        }

        [Test]
        public void publish_is_changed()
        {
            ConfiguredRemotes["primus"].PublishRepositories.Single()
                .Token.ShouldBe("[memory]openwrap");
        }
    }
}