using System.Linq;
using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.Commands.command_line_runner
{
    public class named_input_multiple_values : contexts.command_line_runner
    {
        public named_input_multiple_values()
        {
            given_command("get", "fellows", names => names);
            when_executing("-Names Frodo, \"Samwise Gamgee\"");
        }

        [Test]
        public void command_is_executed()
        {
            CommandExecuted.ShouldBeTrue();
        }

        [Test]
        public void values_are_assigned()
        {
            Input("names").ShouldHaveCountOf(2)
                .Check(_ => _.ElementAt(0).ShouldBe("Frodo"))
                .Check(_ => _.ElementAt(1).ShouldBe("Samwise Gamgee"));
        }
    }
}