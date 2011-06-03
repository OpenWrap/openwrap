using System.Linq;
using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.Commands.command_line_runner
{
    public class positioned_inputs_used_by_name : contexts.command_line_runner
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
        public void command_executes()
        {
            CommandExecuted.ShouldBeTrue();
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
    }
}