using NUnit.Framework;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace Tests.Commands.command_line_locators.noun_verb
{
    [TestFixture("get-ring value")]
    [TestFixture("ring get-value")]
    class token_has_dash : noun_verb_locator
    {
        readonly string _line;

        public token_has_dash(string line)
        {
            _line = line;
            given_command("get", "ring");
            when_executing(line);
        }

        [Test]
        public void command_not_found()
        {
            Result.ShouldBeNull();
        }

        [Test]
        public void line_is_unchanged()
        {
            ResultingLine.ShouldBe(_line);
        }
    }
}