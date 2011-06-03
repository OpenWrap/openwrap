using NUnit.Framework;
using OpenWrap.Commands.Cli.Locators;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace Tests.Commands.command_line_locators.default_verb
{
    class unknown_noun : command_locator<DefaultVerbCommandLocator>
    {
        public unknown_noun()
            : base(_ => new DefaultVerbCommandLocator(_))
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