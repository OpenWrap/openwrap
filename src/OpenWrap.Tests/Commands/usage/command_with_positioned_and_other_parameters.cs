using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.Commands.usage
{
    public class command_with_positioned_and_other_parameters : context.command_description
    {
        public command_with_positioned_and_other_parameters()
        {
            given_command("help", "get");
            given_command_input("positional", typeof(string), true, 0);
            given_command_input("required", typeof(string), true, null);
            given_command_input("required2", typeof(string), true, null);
            given_command_input("optional", typeof(string), false, null);
            when_getting_help();
        }
        [Test]
        public void positionals_then_required_then_optional()
        {
            Help.UsageLine.ShouldBe("get-help [-positional] <String> -required <String> -required2 <String> [-optional <String>]");
        }
    }
}