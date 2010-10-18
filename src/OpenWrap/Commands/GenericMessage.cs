using System;

namespace OpenWrap.Commands
{
    public class GenericMessage : ICommandOutput
    {
        protected string DefaultMessage { get; set; }
        public string Message { get; private set; }
        protected object[] MessageArguments { get; set; }

        public GenericMessage(string message, params object[] args)
        {
            Message = message;
            MessageArguments = args;
            DefaultMessage = "An unknown message was sent.";
        }
        
        public GenericMessage(string message)
        {
            Message = message;
            Type = CommandResultType.Info;
        }
        protected GenericMessage(string message, object[] args, string defaultMessage, CommandResultType type)
        {
            Message = message;
            MessageArguments = args;
            DefaultMessage = defaultMessage;
            Type = type;
        }
        public ICommand Source { get; set; }

        public CommandResultType Type { get; set; }

        public override string ToString()
        {
            
            return MessageArguments != null
                           ? string.Format(Message, MessageArguments)
                           : (Message ?? DefaultMessage);
        }
    }
}