using System;

namespace OpenWrap.Commands
{
    public abstract class AbstractOutput : ICommandOutput
    {
        protected AbstractOutput(string message, params object[] args)
        {
            if (message != null)
                Message = string.Format(message, args);
        }

        public CommandResultType Type { get; set; }
        string Message { get; set; }

        public override string ToString()
        {
            return Message ?? string.Empty;
        }
    }
}