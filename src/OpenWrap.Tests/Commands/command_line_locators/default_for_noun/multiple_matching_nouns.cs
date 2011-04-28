using NUnit.Framework;
using OpenWrap.Commands.Cli.Locators;
using OpenWrap.Testing;

namespace Tests.Commands.command_line_locators.default_for_noun
{
    class multiple_matching_nouns : contexts.command_locator<DefaultForNounCommandLocator>
    {
        public multiple_matching_nouns()
                : base(_ => new DefaultForNounCommandLocator(_))
        {
            given_command("get", "help", command => command.IsDefault = true);
            given_command("get", "helpsystem", command => command.IsDefault = true);
            when_executing("help");
        }

        [Test]
        public void command_is_selected()
        {
            Result.ShouldBeNull();
        }
    }
}