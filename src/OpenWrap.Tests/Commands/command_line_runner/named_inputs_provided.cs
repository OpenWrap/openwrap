using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Testing;

namespace Tests.Commands.runner
{
    public class named_inputs_provided : contexts.command_line_runner
    {
        public named_inputs_provided()
        {
            given_command("get", "help", command => command, verbosity => verbosity);
            when_executing("-command testCommand -verbosity detailed");
        }

        [Test]
        public void all_parameters_are_assigned()
        {
            Input("command").Single().ShouldBe("testCommand");
            Input("verbosity").Single().ShouldBe("detailed");
        }

        [Test]
        public void command_is_executed()
        {
            CommandExecuted.ShouldBeTrue();
        }
    }
}