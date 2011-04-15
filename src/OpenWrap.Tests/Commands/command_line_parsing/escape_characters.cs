using System.Linq;
using NUnit.Framework;
using OpenWrap.Commands.Cli;
using OpenWrap.Testing;

namespace Tests.Commands.command_line_parsing
{
    [TestFixture(@"`-named", "", "-named")]
    [TestFixture(@"-named `""value", "named", "\"value")]
    [TestFixture("-named \"tom `\"bombadil`\"\"", "named", "tom \"bombadil\"")]
    class escape_characters : contexts.input_parser
    {
        readonly string _expectedName;
        readonly string _expectedValue;

        public escape_characters(string line, string expectedName, string expectedValue)
        {
            _expectedName = expectedName;
            _expectedValue = expectedValue;
            when_parsing(line);
        }

        [Test]
        public void name_is_parsed()
        {
            Result.Single().Name.ShouldBe(_expectedName);
        }
        [Test]
        public void value_is_parsed()
        {
            Result.Single().ShouldBeOfType<SingleValueInput>().Value.ShouldBe(_expectedValue);
        }
    }
}