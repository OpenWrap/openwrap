using System.Linq;
using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.Commands.command_line_runner
{
    public class positioned_input_required_provided : contexts.command_line_runner
    {
        public positioned_input_required_provided()
        {
            given_command("get", "help", command => command.Position(0).Required);
            when_executing("destroy-ring");
        }

        [Test]
        public void command_executes()
        {
            CommandExecuted.ShouldBeTrue();
        }

        [Test]
        public void input_is_assigned()
        {
            Input("command").Single().ShouldBe("destroy-ring");
        }
    }
}