using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Commands.Cli;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace Tests.Commands.runner
{
    public class named_input_provided_too_many_times : contexts.command_line_runner
    {
        public named_input_provided_too_many_times()
        {
            given_command("get", "help", command => command);
            when_executing("-command one -command 2");
        }

        [Test]
        public void error_is_displayed()
        {
            Results.ShouldHaveOneOf<CommandInputTooManyTimes>()
                    .InputName.ShouldBe("command");
        }

        [Test]
        public void command_is_not_executed()
        {
            CommandExecuted.ShouldBeFalse();
        }
    }
}