using System.Linq;
using NUnit.Framework;
using OpenWrap.Commands.Cli;
using OpenWrap.Commands.Cli.Parser;
using OpenWrap.Testing;


namespace Tests.Commands.command_line_parsing
{
    class singe_named_input_no_value : contexts.input_parser
    {
        public singe_named_input_no_value()
        {
            when_parsing("-named");
        }

        [Test]
        public void name_is_parsed()
        {
            Result.First().Name.ShouldBe("named");
        }

        [Test]
        public void value_is_empty()
        {
            Result.First().ShouldBeOfType<SingleValueInput>().Value.ShouldBe(string.Empty);
        }
    }
}