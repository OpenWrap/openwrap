using NUnit.Framework;
using OpenWrap.Commands.Cli.Locators;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace Tests.Commands.command_line_locators.noun_verb
{
    class not_enough_tokens : command_locator<NounVerbCommandLocator>
    {
        public not_enough_tokens()
            : base(_ => new NounVerbCommandLocator(_))
        {
            when_executing("ring");
        }

        [Test]
        public void command_not_found()
        {
            Result.ShouldBeNull();
        }

        [Test]
        public void line_is_unchanged()
        {
            ResultingLine.ShouldBe("ring");
        }
    }
}