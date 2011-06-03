using NUnit.Framework;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace Tests.Commands.command_line_locators.noun_verb
{
    class unknown_command : noun_verb_locator
    {
        public unknown_command()
        {
            given_command("get", "food");
            when_executing("ring get");
        }

        [Test]
        public void command_not_found()
        {
            Result.ShouldBeNull();
        }

        [Test]
        public void line_is_unchanged()
        {
            ResultingLine.ShouldBe("ring get");
        }
    }
}