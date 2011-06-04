using NUnit.Framework;
using OpenWrap.Commands.Remote.Messages;
using OpenWrap.Testing;

namespace Tests.Commands.remote.set
{
    public class changing_name_to_existing : contexts.set_remote
    {
        public changing_name_to_existing()
        {
            given_remote_config("primus");
            given_remote_config("secundus");
            when_executing_command("secundus -newname primus");
        }

        [Test]
        public void should_return_error()
        {
            Results.ShouldHaveOne<RemoteNameInUse>()
                .Name.ShouldBe("primus");
        }
    }
}