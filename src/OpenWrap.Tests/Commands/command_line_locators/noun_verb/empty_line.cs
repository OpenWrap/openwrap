using NUnit.Framework;
using OpenWrap.Commands.Cli.Locators;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace Tests.Commands.command_line_locators.noun_verb
{
    class empty_line : command_locator<NounVerbCommandLocator>
    {
        public empty_line() : base(_ => new NounVerbCommandLocator(_))
        {
            when_executing(string.Empty);
        }

        [Test]
        public void command_not_found()
        {
            Result.ShouldBeNull();
        }
    }
}