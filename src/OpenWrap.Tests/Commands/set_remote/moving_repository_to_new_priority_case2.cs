using NUnit.Framework;
using OpenWrap.Testing;
using OpenWrap.Tests.Commands.Remote.Set.context;

namespace OpenWrap.Tests.Commands.Remote.Set
{
    public class moving_repository_to_new_priority_case2 : set_remote
    {
        public moving_repository_to_new_priority_case2()
        {
            when_executing_command("secundus", "-priority", "1");
        }

        [Test]
        public void rearranges_priorities()
        {
            TryGetRepository("secundus").Priority.ShouldBe(1);
            TryGetRepository("primus").Priority.ShouldBe(2);
            TryGetRepository("terz").Priority.ShouldBe(3);
        }
    }
}