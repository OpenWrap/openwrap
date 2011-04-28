using NUnit.Framework;
using OpenWrap.Commands.Cli.Locators;
using OpenWrap.Testing;

namespace Tests.Commands.command_line_locators.default_for_noun
{
    class unknown_noun : contexts.command_locator<DefaultForNounCommandLocator>
    {
        public unknown_noun()
                : base(_ => new DefaultForNounCommandLocator(_))
        {
            given_command("get", "help", command => command.IsDefault = true);
            when_executing("sauron");
        }

        [Test]
        public void command_is_selected()
        {
            Result.ShouldBeNull();
        }
    }
}