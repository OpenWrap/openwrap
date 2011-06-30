using System.Linq;
using NUnit.Framework;
using OpenWrap.Commands.Cli.Parser;
using OpenWrap.Testing;

namespace Tests.Commands.command_line_parsing
{
    [TestFixture("rohan, mordor, 'old forest road'", "rohan", "mordor", "old forest road")]
    [TestFixture("rohan, \"mordor\", 'old forest road'", "rohan", "mordor", "old forest road")]
    [TestFixture("rohan, \"mordor\", 'old forest road'", "rohan", "mordor", "old forest road")]
    [TestFixture("rohan`,, \"mordor,\", 'old forest road, The'", "rohan,", "mordor,", "old forest road, The")]
    class multiple_values : input_parser
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
            Result.Single().ShouldBeOfType<MultiValueInput>().Values.ShouldHaveCountOf(3);
        }

        [Test]
        public void values_are_parsed()
        {
            var values = Result.Single().ShouldBeOfType<MultiValueInput>().Values;
            values.ElementAt(0).ShouldBe(_expectedFirst);
            values.ElementAt(1).ShouldBe(_expectedSecond);
            values.ElementAt(2).ShouldBe(_expectedThird);
        }
    }
}