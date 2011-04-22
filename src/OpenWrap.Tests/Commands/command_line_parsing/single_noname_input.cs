using System.Linq;
using NUnit.Framework;
using OpenWrap.Commands.Cli;
using OpenWrap.Commands.Cli.Parser;
using OpenWrap.Testing;

namespace Tests.Commands.command_line_parsing
{
    class single_noname_input : contexts.input_parser
    {
        public single_noname_input()
        {
            when_parsing("one-ring");
        }

        [Test]
        public void one_input_parsed()
        {
            Result.ShouldHaveCountOf(1);
        }

        [Test]
        public void name_is_empty()
        {
            Result.First().Name.ShouldBe(string.Empty);
        }

        [Test]
        public void value_is_parsed()
        {
            Result.First().ShouldBeOfType<SingleValueInput>().Value.ShouldBe("one-ring");
        }
    }
}