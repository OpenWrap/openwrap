using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenRasta.Wrap.Commands;
using OpenRasta.Wrap.Console;

namespace OpenWrap.Commands
{
    public class CommandList : ICommandResult
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

        public ICommand Command{get; set;}

        public override string ToString()
        {
            return string.Format(Strings.CMD_COMMAND_LIST,
                                 Commands.Aggregate(new StringBuilder(), (sb, c) => sb.AppendLine(c)));
        }
    }
}