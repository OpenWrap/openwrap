using NUnit.Framework;
using OpenWrap.Commands.Cli;
using OpenWrap.Testing;

namespace Tests.Commands.command_line_runner
{
    public class positioned_input_invalid_value : contexts.command_line_runner
    {
        public positioned_input_invalid_value()
        {
            given_command("betray", "fellowship", traitor => traitor.Position(0).AssingmentFails);
            when_executing("Boromir");
        }

        [Test]
        public void error_is_displayed()
        {
            Results.ShouldHaveOne<InputParsingError>()
                .Check(x => x.AttemptedValue.ShouldBe("Boromir")).Check(x => x.InputName.ShouldBe("traitor"));
        }
    }
}