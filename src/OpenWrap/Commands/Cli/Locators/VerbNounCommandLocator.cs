using System;
using System.Linq;

namespace OpenWrap.Commands.Cli.Locators
{
    public class VerbNounCommandLocator : NamedCommandLocator
    {
        public VerbNounCommandLocator(ICommandRepository command)
            : base(command)
        {

        }
        public override ICommandDescriptor Execute(ref string input)
        {
            if (string.IsNullOrEmpty(input)) return null;
            var verbNounToken = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).First();

            var command = FindCommand(verbNounToken);
            if (command == null) return null;
            var lengthToRemove = verbNounToken.Length;
            input = input.Substring(lengthToRemove).TrimStart();
            return command;
        }
    }
}