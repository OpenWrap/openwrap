using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Testing;

namespace Tests.Commands.runner
{
    public class positioned_inputs_used_by_name : contexts.runner
    {
        public positioned_inputs_used_by_name()
        {
            given_command("get",
                          "help",
                          command => command.Position(0),
                          verbosity => verbosity.Position(1));

            when_executing("debug -command destroy-ring");
        }

        [Test]
        public void named_takes_precedence()
        {
            Input("command").Single().ShouldBe("destroy-ring");
        }

        [Test]
        public void positioned_assigned_with_leftovers()
        {
            Input("verbosity").Single().ShouldBe("debug");
        }

        [Test]
        public void command_executes()
        {
            CommandExecuted.ShouldBeTrue();
        }
    }

}