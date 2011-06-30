using System.Linq;
using NUnit.Framework;
using OpenWrap.Commands.Cli;
using OpenWrap.Testing;

namespace Tests.Commands.command_line_runner
{
    public class missing_required_positioned_input : contexts.command_line_runner
    {
        public missing_required_positioned_input()
        {
            given_command("get", "help", command => command.Position(0).Required);
            when_executing("");
        }

        [Test]
        public void command_not_executed()
        {
            CommandExecuted.ShouldBeFalse();
        }

        [Test]
        public void error_is_generated()
        {
            Results.ShouldHaveOne<MissingInput>()
                .MissingInputs.First()
                .Name.ShouldBe("command");
        }
    }
}