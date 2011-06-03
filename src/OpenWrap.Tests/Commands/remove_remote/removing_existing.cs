using NUnit.Framework;
using OpenWrap.Commands.Remote;
using OpenWrap.Commands.Remote.Messages;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace Tests.Commands.remove_remote
{
    public class removing_existing
        : remote_command<RemoveRemoteCommand>
    {
        public removing_existing()
        {
            given_remote_config("esgaroth");
            when_executing_command("esgaroth");
        }

        [Test]
        public void message_confirms_removal()
        {
            Results.ShouldHaveOne<RemoteRemoved>()
                .Name.ShouldBe("esgaroth");
        }

        [Test]
        public void the_remote_repository_is_removed()
        {
            ConfiguredRemotes.ContainsKey("esgaroth").ShouldBeFalse();
        }
    }
}