using NUnit.Framework;
using OpenWrap.Commands.Cli;
using OpenWrap.Testing;

namespace Tests.Commands.command_line_runner
{
    public class named_input_hump_matching_ambiguous : contexts.command_line_runner
    {
        public named_input_hump_matching_ambiguous()
        {
            given_command("get", "help", commandName => commandName, commandValue => commandValue);
            when_executing("-command one-ring");
        }

        [Test]
        public void command_is_not_executed()
        {
            CommandExecuted.ShouldBeFalse();
        }

        [Test]
        public void error_is_returned()
        {
            Results.ShouldHaveOne<AmbiguousInputName>()
                .PotentialInputs.ShouldBe(new[] { "commandName", "commandValue" });
        }
    }
}