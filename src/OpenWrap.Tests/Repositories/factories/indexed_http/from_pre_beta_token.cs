using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.Repositories.factories.indexed_http
{
    public class from_pre_beta_token : contexts.indexed_http_repository
    {
        public from_pre_beta_token()
        {
            when_building_from_token("[indexed]http://middle.earth/");
        }
        [Test]
        public void repository_is_built()
        {
            Repository.ShouldNotBeNull();
        }
        [Test]
        public void token_is_updated()
        {
            Repository.Token.ShouldBe("[indexed-http]http://middle.earth/index.wraplist");
        }
    }
}