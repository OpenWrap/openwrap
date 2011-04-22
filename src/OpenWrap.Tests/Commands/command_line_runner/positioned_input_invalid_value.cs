using NUnit.Framework;
using OpenWrap.Commands;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace Tests.Commands.runner
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
            Results.ShouldHaveOneOf<InputParsingError>()
                    .Check(x => x.AttemptedValue.ShouldBe("Boromir")).Check(x => x.InputName.ShouldBe("traitor"));
        }
    }
}