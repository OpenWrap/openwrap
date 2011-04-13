using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.Commands.command_line_handlers.noun_verb
{
    class empty_line : contexts.noun_verb_handler
    {
        public empty_line()
        {
            when_executing(string.Empty);
        }

        [Test]
        public void command_not_found()
        {
            Result.ShouldBeNull();
        }
    }
    class not_enough_tokens : contexts.noun_verb_handler
    {
        public not_enough_tokens()
        {
            when_executing("ring");
        }

        [Test]
        public void line_is_unchanged()
        {
            ResultingLine.ShouldBe("ring");
        }
        [Test]
        public void command_not_found()
        {
            Result.ShouldBeNull();
        }
    }
}