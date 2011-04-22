using System;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Testing;
using Tests;
using Tests.Commands.contexts;

namespace Tests.Commands.cecil_commands
{
    public class command_with_no_input : cecil_command<CommandWithoutInput>
    {
        public command_with_no_input()
        {
            when_executing(string.Empty);
        }

        [Test]
        public void command_is_executed()
        {
            Command.Executed.ShouldBeTrue();
        }
    }
}