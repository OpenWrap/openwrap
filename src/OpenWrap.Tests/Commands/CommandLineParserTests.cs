using NUnit.Framework;
using OpenWrap.Commands;
using OpenWrap.Testing;

namespace OpenWrap.Tests.Commands
{
    [TestFixture]
    public class CommandLineParserTests
    {
        [Test]
        public void Can_parse_verb_noun_format()
        {
            var parser = new CommandLineParser();
            var commandLine = (parser.Parse(new[] { "add-wrap", "foo", "bar" }) as CommandLineParser.Success).CommandLine;
            commandLine.Noun.ShouldBe("wrap");
            commandLine.Verb.ShouldBe("add");
            commandLine.Arguments.ShouldHaveSameElementsAs(new[] { "foo", "bar" });
        }

        [Test]
        public void Can_parse_noun_verb_format()
        {
            var parser = new CommandLineParser();
            var commandLine = (parser.Parse(new[] { "wrap", "add", "foo", "bar" }) as CommandLineParser.Success).CommandLine;
            commandLine.Noun.ShouldBe("wrap");
            commandLine.Verb.ShouldBe("add");
            commandLine.Arguments.ShouldHaveSameElementsAs(new[] { "foo", "bar" });
        }

        [Test]
        public void Can_parse_verb_noun_format_with_no_arguments()
        {
            var parser = new CommandLineParser();
            var commandLine = (parser.Parse(new[] { "add-wrap" }) as CommandLineParser.Success).CommandLine;
            commandLine.Noun.ShouldBe("wrap");
            commandLine.Verb.ShouldBe("add");
            commandLine.Arguments.ShouldBeEmpty();
        }

        [Test]
        public void Can_parse_noun_verb_format_with_no_arguments()
        {
            var parser = new CommandLineParser();
            var commandLine = (parser.Parse(new[] { "wrap", "add" }) as CommandLineParser.Success).CommandLine;
            commandLine.Noun.ShouldBe("wrap");
            commandLine.Verb.ShouldBe("add");
            commandLine.Arguments.ShouldBeEmpty();
        }

        [Test]
        public void Cannot_parse_empty_command_line()
        {
            var parser = new CommandLineParser();
            var result = parser.Parse(new string[0]) as CommandLineParser.NotEnoughArgumentsFailure;
            result.ShouldNotBeNull();
        }

        [Test]
        public void Cannot_parse_single_argument_without_dash()
        {
            var parser = new CommandLineParser();
            var result = parser.Parse(new[] { "fail" }) as CommandLineParser.NotEnoughArgumentsFailure;
            result.ShouldNotBeNull();
        }
    }
}
