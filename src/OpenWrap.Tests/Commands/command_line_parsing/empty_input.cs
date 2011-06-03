using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.Commands.command_line_parsing
{
    class empty_input : input_parser
    {
        public empty_input()
        {
            when_parsing("");
        }

        [Test]
        public void no_input_is_parsed()
        {
            Result.ShouldBeEmpty();
        }
    }
}