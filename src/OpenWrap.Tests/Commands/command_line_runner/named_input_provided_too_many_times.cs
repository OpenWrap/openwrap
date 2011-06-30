using NUnit.Framework;
using OpenWrap.Commands.Cli;
using OpenWrap.Testing;

namespace Tests.Commands.command_line_runner
{
    public class named_input_provided_too_many_times : contexts.command_line_runner
    {
        public named_input_provided_too_many_times()
        {
            given_command("get", "help", command => command);
            when_executing("-command one -command 2");
        }

        [Test]
        public void command_is_not_executed()
        {
            CommandExecuted.ShouldBeFalse();
        }

        [Test]
        public void error_is_displayed()
        {
            Results.ShouldHaveOne<CommandInputTooManyTimes>()
                .InputName.ShouldBe("command");
        }
    }
}