using System;
using System.Linq;
using NUnit.Framework;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests.Commands.add_remote
{
    public class adding_new_remote_with_separate_fetch_and_publish : contexts.add_remote
    {
        public adding_new_remote_with_separate_fetch_and_publish()
        {
            given_remote_factory(input => new InMemoryRepository(input));
            when_executing_command("iron-hills http://localhost/one -publish http://localhost/two");
        }

        [Test]
        public void fetch_is_set()
        {
            ConfiguredRemotes["iron-hills"].FetchRepository.Token.ShouldBe("[memory]http://localhost/one");
        }

        [Test]
        public void publish_is_set()
        {
            ConfiguredRemotes["iron-hills"].PublishRepositories.Single().Token.ShouldBe("[memory]http://localhost/one");
        }
    }
}