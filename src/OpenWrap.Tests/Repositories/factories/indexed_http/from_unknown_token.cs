using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.Repositories.factories.indexed_http
{
    public class from_unknown_token : contexts.indexed_http_repository
    {
        public from_unknown_token()
        {
            when_building_from_token("[unknown]http://middle.earth/index.wraplist");
        }

        [Test]
        public void repository_is_built()
        {
            Repository.ShouldBeNull();
        }
    }
}