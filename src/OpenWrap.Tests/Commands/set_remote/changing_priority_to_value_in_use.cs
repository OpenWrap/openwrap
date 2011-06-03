using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.Commands.set_remote
{
    public class changing_priority_to_value_in_use : contexts.set_remote
    {
        public changing_priority_to_value_in_use()
        {
            given_remote_config("primus");
            given_remote_config("secundus");
            given_remote_config("terz");
            when_executing_command("terz -priority 1");
        }

        [Test]
        public void repositories_are_moved()
        {
            TryGetRepository("terz").Priority.ShouldBe(1);
            TryGetRepository("primus").Priority.ShouldBe(2);
            TryGetRepository("secundus").Priority.ShouldBe(3);
        }
    }
}