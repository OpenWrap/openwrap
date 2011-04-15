using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Commands;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace Tests.Commands.runner
{
    public class missing_required_positioned_input : contexts.runner
    {
        public missing_required_positioned_input()
        {
            given_command("get", "help", command => command.Position(0).Required);
            when_executing("");
        }

        [Test]
        public void error_is_generated()
        {
            Results.ShouldHaveOneOf<MissingInput>()
                    .MissingInputs.First()
                    .Name.ShouldBe("command");
        }

        [Test]
        public void command_not_executed()
        {
            CommandExecuted.ShouldBeFalse();
        }
    }
}