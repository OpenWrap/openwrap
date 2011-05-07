using NUnit.Framework;
using OpenWrap.Testing;
using OpenWrap.Tests.Commands.Remote.Set.context;

namespace OpenWrap.Tests.Commands.Remote.Set
{
    public class when_changing_remote_priority : set_remote
    {
        public when_changing_remote_priority()
        {
            when_executing_command("secundus", "-priority", "1");
        }

        [Test]
        public void the_second_repository_has_new_priority()
        {
            var remote = TryGetRepository("secundus");
            remote.Priority.ShouldBe(1);
        }
    }
}
