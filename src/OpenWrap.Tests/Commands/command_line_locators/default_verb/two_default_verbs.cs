using NUnit.Framework;
using OpenWrap.Commands.Cli.Locators;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace Tests.Commands.command_line_locators.default_verb
{
    class two_default_verbs : command_locator<DefaultVerbCommandLocator>
    {
        public two_default_verbs()
            : base(_ => new DefaultVerbCommandLocator(_))
        {
            given_command("get", "help", command => command.IsDefault = true);
            given_command("list", "help", command => command.IsDefault = true);
            when_executing("help");
        }

        [Test]
        public void command_is_selected()
        {
            Result.ShouldBeNull();
        }
    }
}