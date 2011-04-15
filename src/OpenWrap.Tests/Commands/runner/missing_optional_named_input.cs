using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Testing;

namespace Tests.Commands.runner
{
    public class missing_optional_named_input : contexts.runner
    {
        public missing_optional_named_input()
        {
            given_command("get", "help", command => command);
            when_executing(string.Empty);
        }

        [Test]
        public void input_not_assigned()
        {
            Input("command").ShouldBeEmpty();
        }

        [Test]
        public void command_is_executed()
        {
            CommandExecuted.ShouldBeTrue();
        }
    }
}