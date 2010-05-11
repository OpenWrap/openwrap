using System;
using OpenRasta.Wrap.Commands;
using OpenWrap;

namespace OpenRasta.Wrap.Console
{
    public class NotEnoughParameters : ICommandResult
    {
        public bool Success
        {
            get { return false; }
        }
        public override string ToString()
        {
            return Strings.CMD_NOT_ENOUGH_ARGS;
        }
        public ICommand Command { get; set; }
    }
    public class Success : ICommandResult
    {
        bool ICommandResult.Success
        {
            get { return true; }
        }
        public override string ToString()
        {
            return "The command executed successfully.";
        }
        public ICommand Command { get; set; }
    }
}