using NUnit.Framework;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace Tests.Commands.command_line_locators.verb_noun
{
    class verb_noun_with_dash : verb_noun_locator
    {
        public verb_noun_with_dash()
        {
            given_command("get", "ring");

            when_executing("get-ring");
        }

        [Test]
        public void command_is_found()
        {
            command_should_be("get", "ring");
        }

        [Test]
        public void verb_noun_removed_from_line()
        {
            ResultingLine.ShouldBe(string.Empty);
        }
    }
}