using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.Commands.cecil_commands
{
    public class command_with_optional_input_provided_without_value : contexts.cecil_command<CommandWithOptionalInput>
    {
        public command_with_optional_input_provided_without_value()
        {
            when_executing("-Towards");
        }

        [Test]
        public void input_not_assigned()
        {
            Command.TowardsAssigned.ShouldBeFalse();
        }
        [Test]
        public void command_is_not_executed()
        {
            Command.Executed.ShouldBeFalse();
        }
    }
}