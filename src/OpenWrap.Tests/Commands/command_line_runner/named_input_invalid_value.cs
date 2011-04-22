using System;
using System.Collections.Generic;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Commands;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace Tests.Commands.runner
{
    public class named_input_invalid_value : contexts.command_line_runner
    {
        public named_input_invalid_value()
        {
            given_command("betray", "fellowship", traitor => traitor.AssingmentFails);
            when_executing("-traitor Boromir");
        }

        [Test]
        public void error_is_displayed()
        {
            Results.ShouldHaveOneOf<InputParsingError>()
                    .Check(x => x.AttemptedValue.ShouldBe("Boromir"))
                    .Check(x => x.InputName.ShouldBe("traitor"));
        }
    }
}