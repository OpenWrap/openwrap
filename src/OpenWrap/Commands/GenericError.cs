using System;

namespace OpenWrap.Commands
{
    public class GenericError : Error
    {
        public GenericError()
        {
        }
        public GenericError(string message)
        {
            Message = message;
        }
        public GenericError(string message, params object[] args)
        {
            MessageFormat = message;
            MessageArguments = args;
        }

        protected object[] MessageArguments { get; set; }

        protected string MessageFormat { get; set; }

        public string Message { get; set; }

        public override string ToString()
        {
            return "An error has occurred: " + (Message ?? string.Format(MessageFormat, MessageArguments));
        }
    }
}