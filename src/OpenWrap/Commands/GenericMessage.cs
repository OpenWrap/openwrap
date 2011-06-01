using System;

namespace OpenWrap.Commands
{
    public class GenericMessage : ICommandOutput
    {
        public GenericMessage(string message, params object[] args)
        {
            Message = message;
            MessageArguments = args;
            DefaultMessage = GetType().Name.CamelToSpacedName();
        }

        public GenericMessage(string message)
        {
            Message = message;
            Type = CommandResultType.Info;
            DefaultMessage = GetType().Name.CamelToSpacedName();
        }

        protected GenericMessage(string message, object[] args, string defaultMessage, CommandResultType type)
        {
            Message = message;
            MessageArguments = args;
            DefaultMessage = defaultMessage;
            Type = type;
        }

        public string Message { get; private set; }

        public ICommand Source { get; set; }

        public CommandResultType Type { get; set; }
        protected string DefaultMessage { get; set; }
        protected object[] MessageArguments { get; set; }

        public override string ToString()
        {
            return MessageArguments != null
                           ? string.Format(Message, MessageArguments)
                           : (Message ?? DefaultMessage);
        }
    }
}