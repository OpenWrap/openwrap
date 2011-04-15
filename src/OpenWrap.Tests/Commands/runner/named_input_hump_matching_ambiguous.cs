using System.Collections.Generic;
using NUnit.Framework;
using OpenWrap.Commands;
using OpenWrap.Testing;
using Tests.Commands.contexts;
using SpecExtensions = OpenWrap.Testing.SpecExtensions;

namespace Tests.Commands.runner
{
    public class named_input_hump_matching_ambiguous : contexts.runner
    {
        public named_input_hump_matching_ambiguous()
        {
            given_command("get", "help", commandName => commandName, commandValue => commandValue);
            when_executing("-command one-ring");
        }

        [Test]
        public void error_is_returned()
        {
            Results.ShouldHaveOneOf<AmbiguousInputName>()
                    .PotentialInputs.ShouldBe(new[] { "commandName", "commandValue" });
        }

        [Test]
        public void command_is_not_executed()
        {
            CommandExecuted.ShouldBeFalse();
        }
    }
}