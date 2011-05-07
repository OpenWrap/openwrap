using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.Repositories.factories.simple_index
{
    public class from_token : contexts.simple_index_repository_factory
    {
        public from_token()
        {
            when_building_from_token("[indexed]http://middle.earth/index.wraplist");
        }

        [Test]
        public void repository_is_built()
        {
            Repository.ShouldNotBeNull();
        }
    }
}