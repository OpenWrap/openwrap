using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Testing;
using Tests;

namespace Tests.Commands.runner
{
    public class mixed_named_and_positioned_inputs : contexts.command_line_runner
    {
        public mixed_named_and_positioned_inputs()
        {
            given_command("be", "evil", doit => doit, towards => towards.Position(0));
            when_executing("middle-earth");
        }

        [Test]
        public void positioned_is_assigned()
        {
            Input("towards").ShouldHaveCountOf(1).First().ShouldBe("middle-earth");
        }
    }
}