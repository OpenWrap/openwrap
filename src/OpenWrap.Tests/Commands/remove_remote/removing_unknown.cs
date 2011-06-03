using NUnit.Framework;
using OpenWrap.Commands.Errors;
using OpenWrap.Commands.Remote;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace Tests.Commands.remove_remote
{
    public class removing_unknown : remote_command<RemoveRemoteCommand>
    {
        public removing_unknown()
        {
            when_executing_command("esgaroth");
        }

        [Test]
        public void an_error_is_returned()
        {
            Results.ShouldHaveOne<UnknownRemoteName>()
                .Name.ShouldBe("esgaroth");
        }
    }
}