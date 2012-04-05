using System;
using System.Linq;
using NUnit.Framework;
using OpenWrap.Commands;
using OpenWrap.Commands.Cli;
using OpenWrap.Testing;

namespace Tests.Commands.command_line_runner
{
    public class unknown_named_input_provided : contexts.command_line_runner
    {
        public unknown_named_input_provided()
        {
            given_command("get", "help");
            when_executing("-command");
        }

        [Test]
        public void command_is_not_executed()
        {
            CommandExecuted.ShouldBeFalse();
        }

        [Test]
        public void should_have_error()
        {
            Results.ShouldHaveOne<UnknownCommandInput>()
                .InputName.ShouldBe("command");
        }
    }
    public class unknown_named_input_with_ExtendedProperties : contexts.command_line_runner
    {
        public unknown_named_input_with_ExtendedProperties()
        {
            given_command<MemoryCommandWithWildcard>("get", "everything");
            when_executing("-now -from everywhere -one for,all");
        }

        [Test]
        public void empty_values_is_present()
        {
            Command.Values["now"].ShouldBeEmpty();
        }

        [Test]
        public void single_value_is_present()
        {
            Command.Values["from"].ShouldBe("everywhere");
        }

        [Test]
        public void multi_values_are_present()
        {
            Command.Values["one"].ShouldBe("for", "all");
        }
        public new MemoryCommandWithWildcard Command { get { return (MemoryCommandWithWildcard)CommandInstance; } }

        public class MemoryCommandWithWildcard : MemoryCommand, ICommandWithWildcards
        {
            public void Wildcards(ILookup<string, string> values)
            {
                Values = values;
            }

            public ILookup<string, string> Values { get; set; }
        }
    }
}