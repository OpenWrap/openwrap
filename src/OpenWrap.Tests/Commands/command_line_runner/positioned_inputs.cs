using System.Linq;
using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.Commands.command_line_runner
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
        public void command_should_execute()
        {
            CommandExecuted.ShouldBeTrue();
        }

        [Test]
        public void inputs_are_assigned()
        {
            Input("command").Single().ShouldBe("destroy-ring");
            Input("verbosity").Single().ShouldBe("verbose");
        }
    }
}