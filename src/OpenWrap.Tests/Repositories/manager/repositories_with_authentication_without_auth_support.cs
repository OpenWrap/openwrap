using System.Linq;
using NUnit.Framework;
using OpenWrap.Repositories;
using OpenWrap.Testing;
using Tests.Repositories.contexts;

namespace Tests.Repositories.manager
{
    public class repositories_with_authentication_without_auth_support : remote_manager
    {
        public repositories_with_authentication_without_auth_support()
        {
            given_remote_factory_memory(x => x.CanAuthenticate = false);
            given_remote_config("iron-hills", fetchUsername: "sauron", fetchPassword: "itsmyprecious");
            when_listing_repositories();
        }

        [Test]
        public void repository_has_no_authentication()
        {
            FetchRepositories.Single().Feature<ISupportAuthentication>().ShouldBeNull();
        }
    }
}