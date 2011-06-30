using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.Commands.remote.set
{
    public class changing_priority_to_new_value : contexts.set_remote
    {
        public changing_priority_to_new_value()
        {
            given_remote_config("primus");
            given_remote_config("secundus");
            given_remote_config("terz");
            when_executing_command("secundus -priority 100");
        }

        [Test]
        public void rearranges_priorities()
        {
            TryGetRepository("primus").Priority.ShouldBe(1);
            TryGetRepository("terz").Priority.ShouldBe(3);
            TryGetRepository("secundus").Priority.ShouldBe(100);
        }
    }
}