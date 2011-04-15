using System.Linq;
using NUnit.Framework;
using OpenWrap.Commands.Cli;
using OpenWrap.Testing;

namespace Tests.Commands.command_line_parsing
{
    [TestFixture("rohan, mordor, 'old forest road'", "rohan", "mordor", "old forest road")]
    [TestFixture("rohan, \"mordor\", 'old forest road'", "rohan", "mordor", "old forest road")]
    [TestFixture("rohan, \"mordor\", 'old forest road'", "rohan", "mordor", "old forest road")]
    [TestFixture("rohan`,, \"mordor,\", 'old forest road, The'", "rohan,", "mordor,", "old forest road, The")]
    class multiple_values : contexts.input_parser
    {
        readonly string _expectedFirst;
        readonly string _expectedSecond;
        readonly string _expectedThird;

        public multiple_values(string line, string expectedFirst, string expectedSecond, string expectedThird)
        {
            _expectedFirst = expectedFirst;
            _expectedSecond = expectedSecond;
            _expectedThird = expectedThird;
            when_parsing(line);
        }

        [Test]
        public void should_have_multiple_values()
        {
            SpecExtensions.ShouldHaveCountOf<string>(Result.Single().ShouldBeOfType<MultiValueInput>().Values, 3);
        }

        [Test]
        public void values_are_parsed()
        {
            var values = SpecExtensions.ShouldBeOfType<MultiValueInput>(Result.Single()).Values;
            SpecExtensions.ShouldBe<string>(values.ElementAt(0), _expectedFirst);
            SpecExtensions.ShouldBe<string>(values.ElementAt(1), _expectedSecond);
            SpecExtensions.ShouldBe<string>(values.ElementAt(2), _expectedThird);
        }
    }
}