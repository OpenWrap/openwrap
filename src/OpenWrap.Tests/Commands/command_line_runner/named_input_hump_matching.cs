using System.Linq;
using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.Commands.runner
{
    public class named_input_hump_matching : contexts.command_line_runner
    {
        public named_input_hump_matching()
        {
            given_command("get", "help", from => from);
            when_executing("-fm Frodo");
        }

        [Test]
        public void input_is_assigned()
        {
            Input("from").Single().ShouldBe("Frodo");
        }

        [Test]
        public void command_executes()
        {
            CommandExecuted.ShouldBeTrue();
        }
    }
}