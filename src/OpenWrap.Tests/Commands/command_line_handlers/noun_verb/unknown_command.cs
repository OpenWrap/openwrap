using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.Commands.command_line_handlers.noun_verb
{
    class unknown_command : contexts.noun_verb_handler
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