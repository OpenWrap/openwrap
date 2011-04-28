using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Commands.Cli.Locators;
using Tests;

namespace Tests.Commands.command_line_locators.default_for_noun
{
    class one_default_verb : contexts.command_locator<DefaultForNounCommandLocator>
    {
        public one_default_verb() : base(_=>new DefaultForNounCommandLocator(_))
        {
            given_command("get", "help", command=>command.IsDefault = true);
            when_executing("help");
        }

        [Test]
        public void command_is_selected()
        {
            command_should_be("get", "help");
        }
    }
}