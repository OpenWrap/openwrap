using NUnit.Framework;
using OpenWrap.Commands;
using OpenWrap.Testing;
using Tests.Commands.contexts;


namespace Tests.Commands.runner
{
    public class unknown_named_input_provided : contexts.command_line_runner
    {
        public unknown_named_input_provided()
        {
            given_command("get", "help");
            when_executing("-command");
        }

        [Test]
        public void should_have_error()
        {
            Results.ShouldHaveOneOf<UnknownCommandInput>()
                    .InputName.ShouldBe("command");
        }

        [Test]
        public void command_is_not_executed()
        {
            CommandExecuted.ShouldBeFalse();
        }
    }
}