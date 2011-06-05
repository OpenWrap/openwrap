using System.Linq;
using NUnit.Framework;
using OpenRasta.Client;
using OpenWrap.Configuration.Remotes.Legacy;
using OpenWrap.Repositories;
using OpenWrap.Testing;
using Tests.Repositories.contexts;

namespace Tests.Repositories.manager.legacy
{
    public class old_config_and_new : remote_manager
    {
        public old_config_and_new()
        {
            given_remote_factory(input => new InMemoryRepository(input.Substring(7, input.Length - 8)), token => new InMemoryRepository(token.Substring("[memory]".Length)));
            given_remote_config("mithlond");
            given_configuration(new RemoteRepositories
            {
                { "iron-hills", new RemoteRepository { Href = "http://iron-hills".ToUri(), Priority = 1 } },
                { "minhiriath", new RemoteRepository { Href = "http://minhiriath".ToUri(), Priority = 2 } }
            });
            when_listing_repositories();
        }

        [Test]
        public void repositories_are_converted()
        {
            FetchRepositories.ShouldHaveCountOf(1).Single().Name.ShouldBe("mithlond");
        }
    }
}