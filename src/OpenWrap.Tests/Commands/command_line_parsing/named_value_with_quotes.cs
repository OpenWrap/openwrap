using System.Linq;
using NUnit.Framework;
using OpenWrap.Commands.Cli;
using OpenWrap.Testing;


namespace Tests.Commands.command_line_parsing
{
    [TestFixture(@"-named "" tom bombadil""", " tom bombadil")]
    [TestFixture(@"-named ' tom bombadil'", " tom bombadil")]
    [TestFixture(@"-named ' tom ""bombadil""'", " tom \"bombadil\"")]
    [TestFixture(@"-named "" tom o'bombadil""", " tom o'bombadil")]
    class named_value_with_quotes : contexts.input_parser
    {
        readonly string _expected;

        public named_value_with_quotes(string line, string expected)
        {
            _expected = expected;
            when_parsing(line);
        }

        [Test]
        public void has_name()
        {
            Result.First().Name.ShouldBe("named");
        }

        [Test]
        public void has_value_preserving_white_space()
        {
            Result.First().ShouldBeOfType<SingleValueInput>()
                    .Value.ShouldBe(_expected);
        }
    }
}