using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Commands;
using OpenWrap.Commands.Cli;
using OpenWrap.Testing;
using Tests.Commands.usage;

namespace Tests.Commands.runner
{
    public class named_input_provided : contexts.runner
    {
        public named_input_provided()
        {
            given_command("get", "help", command => command);
            when_executing("-command test");
        }

        [Test]
        public void parameter_is_assigned()
        {
            Input("command").Single().ShouldBe("test");
        }
    }
    
}