using System;

namespace OpenWrap.Commands
{
    public class GenericMessage : ICommandOutput
    {
        public string Message { get; private set; }
        protected string MessageFormat { get; set; }
        protected object[] MessageArguments { get; set; }

        public GenericMessage(string message, params object[] args)
        {
            MessageFormat = message;
            MessageArguments = args;
        }
        
        public GenericMessage(string message)
        {
            Message = message;
            Type = CommandResultType.Info;
        }
        
        public bool Success
        {
            get { return true; }
        }

        public ICommand Source
        {
            get {return null; }
        }

        public CommandResultType Type { get; set; }

        public override string ToString()
        {
            return (Message ?? string.Format(MessageFormat, MessageArguments));
        }
    }
}