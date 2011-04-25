using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Testing;
using Tests;

namespace Tests.Commands.command_line_runner
{
    public class system_wide_optional_input_on_command_with_it : contexts.command_line_runner
    {
        public system_wide_optional_input_on_command_with_it()
        {
            given_command("get", "help", commandName => commandName, systemRepositoryPath=>systemRepositoryPath);
            given_optional_input("SystemRepositoryPath");
            when_executing("-commandName name -SystemRepositoryPath myPath");
        }

        [Test]
        public void command_is_executed()
        {
            CommandExecuted.ShouldBeTrue();
        }

        [Test]
        public void sys_input_not_set()
        {
            Input("SystemRepositoryPath").Single().ShouldBe("myPath");
        }
    }
}