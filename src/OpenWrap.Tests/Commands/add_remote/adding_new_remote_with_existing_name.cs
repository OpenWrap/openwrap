using NUnit.Framework;
using OpenWrap.Commands.contexts;
using OpenWrap.Commands.Remote;
using OpenWrap.Configuration;

namespace Tests.Commands.add_remote
{
    class adding_new_remote_with_existing_name : command_context<AddRemoteCommand>
    {
        public adding_new_remote_with_existing_name()
        {
            given_remote_configuration(new RemoteRepositories { { "iron-hills", null } });
            when_executing_command("iron-hills http://lotr.org/iron-hills");
        }

        [Test]
        public void an_error_is_returned()
        {
            Results.ShouldHaveError();
        }
    }
}