using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.Commands.usage
{
    public class command_without_options : context.command_description
    {
        public command_without_options()
        {
            given_command("help", "get");
            when_getting_help();
        }
        [Test]
        public void usage_is_correct()
        {
            Help.UsageLine.ShouldBe("get-help");
        }
    }
}
