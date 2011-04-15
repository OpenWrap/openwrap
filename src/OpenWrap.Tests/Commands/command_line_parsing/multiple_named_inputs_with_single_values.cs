using System.Linq;
using NUnit.Framework;
using OpenWrap.Commands.Cli;
using OpenWrap.Testing;

namespace Tests.Commands.command_line_parsing
{
    class multiple_named_inputs_with_single_values : contexts.input_parser
    {
        public multiple_named_inputs_with_single_values()
        {
            when_parsing("-named one-ring -location mordor");
        }

        [Test]
        public void two_inputs_parsed()
        {
            Result.ShouldHaveCountOf(2);
        }

        [Test]
        public void names_are_parsed()
        {
            Result.ElementAt(0).Name.ShouldBe("named");
            Result.ElementAt(1).Name.ShouldBe("location");
        }

        [Test]
        public void values_are_parsed()
        {
            Result.ElementAt(0).ShouldBeOfType<SingleValueInput>().Value.ShouldBe("one-ring");
            Result.ElementAt(1).ShouldBeOfType<SingleValueInput>().Value.ShouldBe("mordor");

        }

    }
}