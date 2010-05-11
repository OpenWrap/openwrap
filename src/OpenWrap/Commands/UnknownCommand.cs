using System.Collections.Generic;
using OpenRasta.Wrap.Commands;
using OpenWrap;

namespace OpenRasta.Wrap.Console
{
    public class UnknownCommand : ICommandResult
    {
        public string CommandName { get; set; }

        public UnknownCommand(string commandName, List<string> verbs)
        {
            CommandName = commandName;
        }

        public bool Success
        {
            get { return false; }
        }
        public override string ToString()
        {
            return string.Format(Strings.CMD_UNKNOWN, CommandName);
        }
        public ICommand Command { get; set; }
    }
}