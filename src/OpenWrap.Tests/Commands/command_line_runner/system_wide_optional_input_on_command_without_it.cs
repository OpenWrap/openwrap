using System;
using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.Commands.command_line_runner
{
    public class system_wide_optional_input_on_command_without_it : contexts.command_line_runner
    {
        public system_wide_optional_input_on_command_without_it()
        {
            given_command("get", "help", commandName => commandName);
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
            Input("SystemRepositoryPath").ShouldBeEmpty();
        }
    }
    public class system_wide_optional_input_on_command_without_inputs : contexts.command_line_runner
    {
        public system_wide_optional_input_on_command_without_inputs()
        {
            given_command("get", "help");
            given_optional_input("debug");
            when_executing("-debug");
        }

        [Test]
        public void command_is_executed()
        {
            CommandExecuted.ShouldBeTrue();

        }
    }
}