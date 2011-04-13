using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenWrap.Commands.Cli
{
    public class VerbNounCommandLineHandler : ICommandLineHandler
    {
        readonly ICommandRepository _commands;

        public VerbNounCommandLineHandler(ICommandRepository commands)
        {
            _commands = commands;
        }

        public ICommandDescriptor Execute(ref string input)
        {
            var verbNounToken = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            if (verbNounToken == null) return null;

            var commands = _commands.ToDictionary(x => x.Verb + "-" + x.Noun);

            var matching = verbNounToken.Contains('-')
                                   ? verbNounToken.SelectHumps(commands.Keys).SingleOrDefault()
                                   : ("get-" + verbNounToken).SelectHumps(commands.Keys).SingleOrDefault()
                                     ?? ("list-" + verbNounToken).SelectHumps(commands.Keys).SingleOrDefault();
            if (matching == null) return null;

            input = input.Substring(verbNounToken.Length).TrimStart();
            return commands[matching];
        }
    }
}