using NUnit.Framework;
using OpenWrap.Commands.Cli.Locators;
using OpenWrap.Testing;

namespace Tests.Commands.command_line_locators.default_for_noun
{
    class two_default_verbs : contexts.command_locator<DefaultForNounCommandLocator>
    {
        public two_default_verbs()
                : base(_ => new DefaultForNounCommandLocator(_))
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