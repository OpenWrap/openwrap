using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.Commands.usage
{
    public class command_with_positioned_and_optional_parameters : context.command_description
    {
        public command_with_positioned_and_optional_parameters()
        {
            given_command("help", "get");
            given_command_input("first", typeof(string), true, 0);
            given_command_input("second", typeof(int), false, 1);
            when_getting_help();
        }
        [Test]
        public void usage_line_is_correct()
        {
            Help.UsageLine.ShouldBe("get-help [-first] <String> [[-second] <Int32>]");
        }
    }
}