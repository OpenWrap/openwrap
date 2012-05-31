using System;
using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.Commands.command_line_runner
{
    public class system_wide_optional_inputs_and_empty_positioned : contexts.command_line_runner
    {
        public system_wide_optional_inputs_and_empty_positioned()
        {
            given_command("add", "wrap", 
                          name => name.Position(0).Required,
                          version => version.Position(1).Required.Type<Version>());
            
            given_optional_input("ShellDebug");
            when_executing("nunit -shelldebug");
        }
        [Test]
        public void command_is_not_executed()
        {
            CommandExecuted.ShouldBeFalse();
        }
    }
}