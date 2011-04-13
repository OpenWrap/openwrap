using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.Commands.command_line_handlers.noun_verb
{
    class noun_verb_with_space : contexts.noun_verb_handler
    {
        public noun_verb_with_space()
        {
            given_command("get", "ring");
            
            when_executing("ring get");
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