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
            FetchRepositories.Single().ShouldBeOfType<PreAuthenticatedRepository>()
                .Credentials
                .Check(_ => _.UserName.ShouldBe("sauron"))
                .Check(_ => _.Password.ShouldBe("itsmyprecious"));
        }

        [Test]
        public void repository_supports_credentials_override()
        {
            var repo = FetchRepositories.OfType<PreAuthenticatedRepository>().Single();
            var auth = repo.Feature<ISupportAuthentication>()
                .WithCredentials(new NetworkCredential("saruman", "impersonator"));
            repo.Credentials
                .Check(_ => _.UserName.ShouldBe("saruman"))
                .Check(_ => _.Password.ShouldBe("impersonator"));

            auth.Dispose();
            repo.Credentials
                .Check(_ => _.UserName.ShouldBe("sauron"))
                .Check(_ => _.Password.ShouldBe("itsmyprecious"));
        }
    }
}