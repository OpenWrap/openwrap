using System;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Testing;
using Tests;

namespace Tests.Commands.cecil_commands
{
    public class command_with_optional_input_not_provided : contexts.cecil_command<CommandWithOptionalInput>
    {
        public command_with_optional_input_not_provided()
        {
            when_executing(string.Empty);
        }

        [Test]
        public void input_not_assigned()
        {
            Command.TowardsAssigned.ShouldBeFalse();
        }
        [Test]
        public void command_is_executed()
        {
            Command.Executed.ShouldBeTrue();
        }
    }
}