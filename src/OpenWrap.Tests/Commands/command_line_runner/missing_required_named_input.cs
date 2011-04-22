using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Commands;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace Tests.Commands.runner
{
    public class missing_required_named_input : contexts.command_line_runner
    {
        public missing_required_named_input()
        {
            given_command("get", "help", command => command.Required);
            when_executing(string.Empty);
        }

        [Test]
        public void error_is_displayed()
        {
            Results.ShouldHaveOneOf<MissingInput>()
                    .MissingInputs.ShouldHaveCountOf(1)
                    .Single().Name.ShouldBe("command");
        }

        [Test]
        public void command_is_not_executed()
        {
            CommandExecuted.ShouldBeFalse();
        }
    }
        
}