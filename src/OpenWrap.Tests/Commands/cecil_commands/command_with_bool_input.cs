using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Commands;
using OpenWrap.Testing;
using Tests;

namespace Tests.Commands.cecil_commands
{
    public class command_with_bool_input : contexts.cecil_command<CommandWithBoolInput>
    {
        public command_with_bool_input()
        {
            when_executing("-DoIt");
        }

        [Test]
        public void boolean_inputs_have_optional_values_by_default()
        {
            Command.DoIt.ShouldBeTrue();
            Command.Executed.ShouldBeTrue();
        }
    }

    [Command]
    public class CommandWithBoolInput : TestableCommand
    {
        [CommandInput]
        public bool DoIt { get; set; }
    }
}