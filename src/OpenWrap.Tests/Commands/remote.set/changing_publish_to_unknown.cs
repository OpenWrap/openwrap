using NUnit.Framework;
using OpenWrap.Commands.Remote.Messages;
using OpenWrap.Testing;

namespace Tests.Commands.remote.set
{
    public class changing_publish_to_unknown : contexts.set_remote
    {
        public changing_publish_to_unknown()
        {
            given_remote_config("primus");
            when_executing_command("primus -publish openwrap");
        }

        [Test]
        public void error_is_returned()
        {
            Results.ShouldHaveOne<UnknownEndpointType>()
                .Path.ShouldBe("openwrap");
        }
    }
}