using NUnit.Framework;
using OpenWrap.Commands.Remote.Messages;
using OpenWrap.Testing;

namespace Tests.Commands.add_remote
{
    class adding_new_remote_with_existing_name : contexts.add_remote
    {
        public adding_new_remote_with_existing_name()
        {
            given_remote_config("iron-hills");
            when_executing_command("iron-hills http://lotr.org/iron-hills");
        }

        [Test]
        public void an_error_is_returned()
        {
            Results.ShouldHaveOne<RemoteNameInUse>()
                .Name.ShouldBe("iron-hills");
        }
    }
}