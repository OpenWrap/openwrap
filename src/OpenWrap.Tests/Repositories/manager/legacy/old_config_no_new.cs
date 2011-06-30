using System;
using System.Linq;
using NUnit.Framework;
using OpenRasta.Client;
using OpenWrap.Repositories;
using OpenWrap.Testing;
using Tests.Repositories.contexts;
using LegacyRemotes = OpenWrap.Configuration.Remotes.Legacy.RemoteRepositories;
using LegacyRemote = OpenWrap.Configuration.Remotes.Legacy.RemoteRepository;

namespace Tests.Repositories.manager.legacy
{
    public class old_config_no_new : remote_manager
    {
        public old_config_no_new()
        {
            given_remote_factory(
                input => new InMemoryRepository(input.Substring(7, input.Length - 8)) { CanPublish = input == "http://iron-hills/" },
                token => new InMemoryRepository(token.Substring("[memory]".Length)));
            given_configuration(new LegacyRemotes
            {
                { "iron-hills", new LegacyRemote { Href = "http://iron-hills".ToUri(), Priority = 1 } },
                { "minhiriath", new LegacyRemote { Href = "http://minhiriath".ToUri(), Priority = 2 } }
            });
            Environment.ConfigurationDirectory.GetFile("remotes").Delete();
            when_listing_repositories();
        }

        [Test]
        public void new_config_file_is_created()
        {
            Environment.ConfigurationDirectory.GetFile("remotes").Exists.ShouldBeTrue();
        }

        [Test]
        public void priorities_are_converted()
        {
            ConfiguredRemotes["iron-hills"].Priority.ShouldBe(1);
            ConfiguredRemotes["minhiriath"].Priority.ShouldBe(2);
        }

        [Test]
        public void repository_fetches_are_converted()
        {
            FetchRepositories.ShouldHaveCountOf(2)
                .Check(_ => _.ElementAt(0).Name.ShouldBe("iron-hills"))
                .Check(_ => _.ElementAt(1).Name.ShouldBe("minhiriath"));
        }

        [Test]
        public void repository_publishing_are_converted()
        {
            PublishRepositories.ShouldHaveCountOf(1)
                .Single().Name.ShouldBe("iron-hills");
        }
    }
}