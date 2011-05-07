using System.Collections.Generic;
using NUnit.Framework;
using OpenWrap.Repositories;
using OpenWrap.Testing;
using Tests;

namespace Tests.Repositories.factories
{

    public class from_user_input_index_exists : contexts.simple_index_repository_factory
    {
        public from_user_input_index_exists()
        {
            given_remote_resource("http://middle.earth/index.wraplist", "application/vnd.openwrap.index+xml", "<package-list />");
            when_detecting("http://middle.earth");
        }

        [Test]
        public void repository_is_built()
        {
            Repository.ShouldNotBeNull();
        }

        [Test]
        public void token_is_generated()
        {
            Repository.Token.ShouldBe("[indexed]http://middle.earth/index.wraplist");
        }
    }
}

namespace Tests.contexts
{
}