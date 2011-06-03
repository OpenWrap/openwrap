using NUnit.Framework;
using OpenWrap.Commands.Cli;
using OpenWrap.Testing;

namespace Tests.Commands.command_line_runner
{
    public class unknown_named_input_provided : contexts.command_line_runner
    {
        public unknown_named_input_provided()
        {
            given_command("get", "help");
            when_executing("-command");
        }

        [Test]
        public void command_is_not_executed()
        {
            CommandExecuted.ShouldBeFalse();
        }

        [Test]
        public void should_have_error()
        {
            Results.ShouldHaveOne<UnknownCommandInput>()
                .InputName.ShouldBe("command");
        }
    }
}