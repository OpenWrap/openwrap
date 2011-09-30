using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenWrap.Commands;
using OpenWrap.Commands.Cli;
using OpenWrap.Commands.Cli.Locators;
using Tests.Commands.usage;

namespace Tests.Commands.contexts
{
    abstract class command_locator<T> : command_locator where T: ICommandLocator
    {
        public command_locator(Func<ICommandRepository, T> builder)
        {
            Repository = new CommandRepository();
            Handler = builder(Repository);
        }
    }

    abstract class verb_noun_locator : command_locator<VerbNounCommandLocator>
    {
        public verb_noun_locator()
            : base(_ => new VerbNounCommandLocator(_))
        {
        }
    }
}
