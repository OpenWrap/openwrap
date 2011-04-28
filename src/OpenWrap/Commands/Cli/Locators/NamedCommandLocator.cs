using System;
using System.Linq;

namespace OpenWrap.Commands.Cli.Locators
{
    public abstract class NamedCommandLocator : ICommandLocator
    {
        readonly ICommandRepository _commands;

        public NamedCommandLocator(ICommandRepository commands)
        {
            _commands = commands;
        }

        public ICommandDescriptor FindCommand(string verbNounToken)
        {
            var commands = _commands.Distinct(CommandDescriptorComparer.VerbNoun).ToDictionary(x => x.Verb + "-" + x.Noun);


            var matching =  verbNounToken.Contains('-')
                                   ? verbNounToken.SelectHumps(commands.Keys).SingleOrDefault()
                                   : null;
            return matching == null ? null : commands[matching];
        }

        public abstract ICommandDescriptor Execute(ref string input);
    }
}