using NUnit.Framework;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace Tests.Commands.command_line_locators.verb_noun
{
    class unknown_verb_noun : verb_noun_locator
    {
        public unknown_verb_noun()
        {
            given_command("get", "help");
            when_executing("get-ring");
        }

        [Test]
        public void command_not_found()
        {
            Result.ShouldBeNull();
        }
    }
}