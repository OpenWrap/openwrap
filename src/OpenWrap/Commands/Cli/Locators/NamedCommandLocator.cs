using System.Linq;

namespace OpenWrap.Commands.Cli
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

            var commands = _commands.ToDictionary(x => x.Verb + "-" + x.Noun);

            var matching = verbNounToken.Contains('-')
                                   ? verbNounToken.SelectHumps(commands.Keys).SingleOrDefault()
                                   : ("get-" + verbNounToken).SelectHumps(commands.Keys).SingleOrDefault()
                                     ?? ("list-" + verbNounToken).SelectHumps(commands.Keys).SingleOrDefault();
            return matching == null ? null : commands[matching];
        }

        public abstract ICommandDescriptor Execute(ref string input);
    }
}