using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenRasta.Client.Memory;
using OpenWrap; 
using OpenWrap.Testing;
using Tests;

namespace Tests.Repositories.factories.nuget
{
    public class from_user_input_by_name : contexts.nuget_repository_factory
    {
        public from_user_input_by_name()
        {
            given_remote_resource(
                "https://go.microsoft.com/fwlink/?LinkID=206669",
                GET => new MemoryResponse(307) { Headers = { { "Location", "http://middle.earth/mordor/nuget" } } }
                );
            given_remote_resource(
                    "http://middle.earth/mordor/nuget",
                    "application/atom+xml",
                    AtomContent.ServiceDocument("http://middle.earth/mordor/nuget/", "Packages")
                    );
            when_detecting("nuget");
        }

        [Test]
        public void token_contains_resolved_uri()
        {
            Repository.Token.ShouldBe("[nuget][https://go.microsoft.com/fwlink/?LinkID=206669]http://middle.earth/mordor/nuget/Packages");
        }

        [Test]
        public void repository_should_be_constructed()
        {
            Repository.ShouldNotBeNull();
        }
    }
}