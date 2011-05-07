using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Testing;
using Tests;

namespace Tests.Repositories.factories.simple_index
{
    public class from_unknown_token : contexts.simple_index_repository_factory
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