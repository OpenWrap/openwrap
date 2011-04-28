using NUnit.Framework;
using OpenWrap.Commands.Cli.Locators;
using OpenWrap.Testing;

namespace Tests.Commands.command_line_locators.noun_verb
{
    class noun_verb_with_space : contexts.command_locator<NounVerbCommandLocator>
    {
        public noun_verb_with_space()
            : base(_ => new NounVerbCommandLocator(_))
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