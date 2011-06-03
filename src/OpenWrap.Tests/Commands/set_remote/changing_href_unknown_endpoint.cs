using System;
using NUnit.Framework;
using OpenWrap.Commands.Remote.Messages;
using OpenWrap.Testing;

namespace Tests.Commands.set_remote
{
    public class changing_href_unknown_endpoint : contexts.set_remote
    {
        public changing_href_unknown_endpoint()
        {
            given_remote_config("secundus");
            when_executing_command("secundus -href unknown://awesomereps.net");
        }

        [Test]
        public void error_is_returned()
        {
            Results.ShouldHaveOne<UnknownEndpointType>()
                .Path.ShouldBe("unknown://awesomereps.net");
        }
    }
}