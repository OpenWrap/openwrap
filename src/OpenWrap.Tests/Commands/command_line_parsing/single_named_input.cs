using System.Linq;
using NUnit.Framework;
using OpenWrap.Commands.Cli;
using OpenWrap.Testing;

namespace Tests.Commands.command_line_parsing
{
    class single_named_input : contexts.input_parser
    {
        public single_named_input()
        {
            when_parsing("-named one-ring");
        }
        [Test]
        public void one_input_parsed()
        {
            Result.ShouldHaveCountOf(1);
        }
        [Test]
        public void name_is_parsed()
        {
            Result.First().Name.ShouldBe("named");
        }
        [Test]
        public void value_is_parsed()
        {
            Result.First().ShouldBeOfType<SingleValueInput>().Value.ShouldBe("one-ring");
        }
    }
}