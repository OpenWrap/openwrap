using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.Commands.command_line_handlers.verb_noun
{
    class unknown_verb_noun : contexts.verb_noun_handler
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