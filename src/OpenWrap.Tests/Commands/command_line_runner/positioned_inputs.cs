using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Testing;

namespace Tests.Commands.runner
{
    public class positioned_inputs : contexts.command_line_runner
    {
        public positioned_inputs()
        {
            given_command("get",
                          "help",
                          command => command.Position(0),
                          verbosity => verbosity.Position(1));
            when_executing("destroy-ring verbose");
        }

        [Test]
        public void inputs_are_assigned()
        {
            Input("command").Single().ShouldBe("destroy-ring");
            Input("verbosity").Single().ShouldBe("verbose");
        }

        [Test]
        public void command_should_execute()
        {
            CommandExecuted.ShouldBeTrue();
        }
    }

    public class positioned_input_required_provided : contexts.command_line_runner
    {
        public positioned_input_required_provided()
        {
            given_command("get",
                          "help",
                          command => command.Position(0).Required);
            when_executing("destroy-ring");
        }

        [Test]
        public void input_is_assigned()
        {
            Input("command").Single().ShouldBe("destroy-ring");
        }

        [Test]
        public void command_executes()
        {
            CommandExecuted.ShouldBeTrue();
        }
    }
}