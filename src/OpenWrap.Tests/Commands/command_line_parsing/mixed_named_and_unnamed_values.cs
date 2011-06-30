using System.Linq;
using NUnit.Framework;
using OpenWrap.Commands.Cli.Parser;
using OpenWrap.Testing;

namespace Tests.Commands.command_line_parsing
{
    [TestFixture("somewhere -named one-ring -evil")]
    [TestFixture("-evil -named one-ring somewhere")]
    [TestFixture("-named one-ring somewhere -evil")]
    class mixed_named_and_unnamed_values : input_parser
    {
        public mixed_named_and_unnamed_values(string line)
        {
            when_parsing(line);
        }

        [Test]
        public void named_with_value_parsed()
        {
            Result.FirstOrDefault(x => x.Name == "named")
                .ShouldNotBeNull()
                .ShouldBeOfType<SingleValueInput>()
                .Value.ShouldBe("one-ring");
        }

        [Test]
        public void named_without_value_parsed()
        {
            Result.FirstOrDefault(x => x.Name == "evil")
                .ShouldNotBeNull()
                .ShouldBeOfType<SingleValueInput>()
                .Value.ShouldBe(string.Empty);
        }

        [Test]
        public void value_without_name_parsed()
        {
            Result.FirstOrDefault(x => x.Name == string.Empty)
                .ShouldNotBeNull()
                .ShouldBeOfType<SingleValueInput>()
                .Value.ShouldBe("somewhere");
        }
    }
}