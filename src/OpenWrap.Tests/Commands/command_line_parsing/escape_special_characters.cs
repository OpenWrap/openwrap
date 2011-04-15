using System.Linq;
using NUnit.Framework;
using OpenWrap.Commands.Cli;
using OpenWrap.Testing;


namespace Tests.Commands.command_line_parsing
{
    [TestFixture("`0", "\0")]
    [TestFixture("`a", "\a")]
    [TestFixture("`b", "\b")]
    [TestFixture("`f", "\f")]
    [TestFixture("`n", "\n")]
    [TestFixture("`r", "\r")]
    [TestFixture("`t", "\t")]
    [TestFixture("`v", "\v")]
    class escape_special_characters : contexts.input_parser
    {
        readonly string _expected;

        public escape_special_characters(string line, string expected)
        {
            _expected = expected;
            when_parsing(line);
        }

        [Test]
        public void special_character_appended()
        {
            Result.Single().ShouldBeOfType<SingleValueInput>().Value.ShouldBe(_expected);
        }
    }
}