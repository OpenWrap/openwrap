using System;

namespace OpenWrap.Commands
{
    public class Success : ICommandOutput
    {
        public Success()
        {
            Type = CommandResultType.Default;

        }
        bool ICommandOutput.Success
        {
            get { return true; }
        }
        public override string ToString()
        {
            return "The command executed successfully.";
        }
        public ICommand Source { get; set; }

        public CommandResultType Type { get; protected set; }
    }
}