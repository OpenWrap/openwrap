using System;
using System.Linq;

namespace OpenWrap.Commands.Cli
{
    public class NounVerbCommandLineHandler : NamedCommandLineHandler
    {
        public NounVerbCommandLineHandler(ICommandRepository commands) : base(commands)
        {
        }

        public override ICommandDescriptor Execute(ref string input)
        {
            if (string.IsNullOrEmpty(input)) return null;
            var tokens = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 2) return null;

            if (tokens[0].Contains('-')) return null;
            var verbNounToken = tokens[1] + "-" + tokens[0];
            var command = FindCommand(verbNounToken);
            if (command == null) return null;
            input = input.Substring(input.IndexOf(tokens[1]) + tokens[1].Length).TrimStart();
            return command;
        }
    }
}