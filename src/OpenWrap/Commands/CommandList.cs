using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenWrap.Commands;

namespace OpenWrap.Commands
{
    public class CommandList : ICommandOutput
    {
        public ICollection<string> Commands { get; set; }

        public CommandList(ICollection<string> commands)
        {
            Commands = commands;
        }

        public bool Success
        {
            get { return true; }
        }

        public ICommand Source{get; set;}

        public CommandResultType Type
        {
            get { return CommandResultType.Info; }
        }

        public override string ToString()
        {
            return string.Format(Strings.CMD_COMMAND_LIST,
                                 Commands.Aggregate(new StringBuilder(), (sb, c) => sb.AppendLine(c)));
        }
    }
}