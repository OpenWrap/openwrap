using NUnit.Framework;
using OpenWrap.Commands;
using OpenWrap.Tests.Commands.Remote.Set.context;

namespace OpenWrap.Tests.Commands.Remote.Set
{
    public class when_changing_repository_name_to_existing : set_remote
    {
        public when_changing_repository_name_to_existing()
        {
            when_executing_command("secundus", "-newname", "primus");
        }

        [Test]
        public void should_return_error()
        {
            Results.ShouldContain<Error>();
        }
    }
}