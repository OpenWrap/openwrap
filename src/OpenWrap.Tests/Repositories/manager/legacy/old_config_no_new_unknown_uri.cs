using NUnit.Framework;
using OpenRasta.Client;
using OpenWrap.Configuration.Remotes.Legacy;
using OpenWrap.Repositories;
using OpenWrap.Testing;
using Tests.Repositories.contexts;

namespace Tests.Repositories.manager.legacy
{
    public class old_config_no_new_unknown_uri : remote_manager
    {
        public old_config_no_new_unknown_uri()
        {
            given_remote_factory(input => null, token => new InMemoryRepository(token.Substring("[memory]".Length)));
            given_configuration(new RemoteRepositories
            {
                { "iron-hills", new RemoteRepository { Href = "http://iron-hills".ToUri(), Priority = 1 } }
            });
            Environment.ConfigurationDirectory.GetFile("remotes").Delete();
            when_listing_repositories();
        }

        [Test]
        public void repositories_are_ignored()
        {
            FetchRepositories.ShouldBeEmpty();
            PublishRepositories.ShouldBeEmpty();
        }
    }
}