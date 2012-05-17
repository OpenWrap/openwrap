using System;
using System.Linq;
using System.Net;
using NUnit.Framework;
using OpenWrap.Repositories;
using OpenWrap.Testing;
using Tests.Repositories.contexts;

namespace Tests.Repositories.manager
{
    public class repositories_with_authentication : remote_manager
    {
        public repositories_with_authentication()
        {
            given_remote_factory_memory(x => x.CanAuthenticate = true);
            given_remote_config("iron-hills", fetchUsername: "sauron", fetchPassword: "itsmyprecious");
            when_listing_repositories();
        }

        [Test]
        public void repository_has_authentication()
        {
            FetchRepositories.Single().Feature<ISupportAuthentication>()
                .CurrentCredentials
                .Check(_ => _.UserName.ShouldBe("sauron"))
                .Check(_ => _.Password.ShouldBe("itsmyprecious"));
        }

        [Test]
        public void repository_supports_credentials_override()
        {
            var repo = FetchRepositories.Single().Feature<ISupportAuthentication>();
            var auth = repo.WithCredentials(new NetworkCredential("saruman", "impersonator"));
            repo.CurrentCredentials
                .Check(_ => _.UserName.ShouldBe("saruman"))
                .Check(_ => _.Password.ShouldBe("impersonator"));

            auth.Dispose();
            repo.CurrentCredentials
                .Check(_ => _.UserName.ShouldBe("sauron"))
                .Check(_ => _.Password.ShouldBe("itsmyprecious"));
        }
    }
}