using NUnit.Framework;
using OpenWrap.Testing;
using OpenWrap.Tests.Commands.Remote.Set.context;

namespace OpenWrap.Tests.Commands.Remote.Set
{
    public class moving_repository_to_new_priority_case4 : set_remote
    {
        public moving_repository_to_new_priority_case4()
        {
            when_executing_command("secundus -priority 3");
        }

        [Test]
        public void rearranges_priorities()
        {
            TryGetRepository("primus").Priority.ShouldBe(1);
            TryGetRepository("terz").Priority.ShouldBe(2);
            TryGetRepository("secundus").Priority.ShouldBe(3);
        }
    }
}