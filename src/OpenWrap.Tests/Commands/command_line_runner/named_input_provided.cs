using System.Linq;
using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.Commands.command_line_runner
{
    public class named_input_provided : contexts.command_line_runner
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