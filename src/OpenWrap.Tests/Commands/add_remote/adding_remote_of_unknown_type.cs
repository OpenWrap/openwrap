using NUnit.Framework;
using OpenWrap.Commands.Remote;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace Tests.Commands.add_remote
{
    public class adding_remote_of_unknown_type : remote_command<AddRemoteCommand>
    {
        public adding_remote_of_unknown_type()
        {
            when_executing_command("iron-hills http://lotr.org/iron-hills");
        }

        [Test]
        public void config_not_persisted()
        {
            ConfiguredRemotes.Keys.ShouldNotContain("iron-hills");
        }
        [Test]
        public void error_is_returned()
        {
            Results.ShouldHaveError();
        }
    }
}