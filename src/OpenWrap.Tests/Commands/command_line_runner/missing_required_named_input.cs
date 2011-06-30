using System.Linq;
using NUnit.Framework;
using OpenWrap.Commands.Cli;
using OpenWrap.Testing;

namespace Tests.Commands.command_line_runner
{
    public class missing_required_named_input : contexts.command_line_runner
    {
        public missing_required_named_input()
        {
            given_command("get", "help", command => command.Required);
            when_executing(string.Empty);
        }

        [Test]
        public void command_is_not_executed()
        {
            CommandExecuted.ShouldBeFalse();
        }

        [Test]
        public void error_is_displayed()
        {
            Results.ShouldHaveOne<MissingInput>()
                .MissingInputs.ShouldHaveCountOf(1)
                .Single().Name.ShouldBe("command");
        }
    }
}