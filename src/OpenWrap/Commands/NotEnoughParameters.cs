using System;
using OpenWrap.Commands;
using OpenWrap;

namespace OpenWrap.Commands
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