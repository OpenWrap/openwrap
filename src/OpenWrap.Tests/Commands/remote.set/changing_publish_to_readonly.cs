using NUnit.Framework;
using OpenWrap.Commands.Remote.Messages;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests.Commands.remote.set
{
    public class changing_publish_to_readonly : contexts.set_remote
    {
        public changing_publish_to_readonly()
        {
            given_remote_config("primus");
            given_remote_factory_memory(repo => repo.CanPublish = false);

            when_executing_command("primus -publish openwrap");
        }

        [Test]
        public void error_is_returned()
        {
            Results.ShouldHaveOne<RemoteEndpointReadOnly>()
                .Path.ShouldBe("openwrap");
        }
    }
}