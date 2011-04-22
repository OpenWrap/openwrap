using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.Commands.cecil_commands
{
    public class command_with_input_optional_value_provided_without_value : contexts.cecil_command<CommandWithOptionalInputValue>
    {
        public command_with_input_optional_value_provided_without_value()
        {
            when_executing("-DoIt");
        }

        [Test]
        public void command_is_executed()
        {
            Command.Executed.ShouldBeTrue();
        }

        [Test]
        public void input_assigned()
        {
            Command.DoItAssigned.ShouldBeTrue();
        }
    }
}