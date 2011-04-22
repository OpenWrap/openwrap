using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.Commands.cecil_commands
{
    public class command_with_optional_input_provided_with_value : contexts.cecil_command<CommandWithOptionalInput>
    {
        public command_with_optional_input_provided_with_value()
        {
            when_executing("-Towards MiddleEarth");
        }

        [Test]
        public void input_assigned()
        {
            Command.TowardsAssigned.ShouldBeTrue();
        }
        [Test]
        public void command_is_executed()
        {
            Command.Executed.ShouldBeTrue();
        }
    }
}