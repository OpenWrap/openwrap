using NUnit.Framework;
using OpenWrap.Commands.Remote.Messages;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests.Commands.remote.add
{
    public class adding_new_remote_with_invalid_name : contexts.add_remote
    {
        public adding_new_remote_with_invalid_name()
        {
            given_remote_factory_memory();

            when_executing_command("\"iron hills\" http://lotr.org/iron-hills");
        }

        [Test]
        public void error_is_returned()
        {
            Results.ShouldHaveOne<RemoteNameInvalid>()
                .Name.ShouldBe("iron hills");
        }
    }
}