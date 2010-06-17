using System;
using OpenWrap.Commands;

namespace OpenWrap.Commands.Wrap
{
    public class GenericMessage : ICommandResult
    {
        public string Message { get; set; }

        public GenericMessage(string message)
        {
            Message = message;
        }

        public bool Success
        {
            get { return true; }
        }

        public ICommand Command
        {
            get {return null; }
        }
    }

    public class GenericError : Error
    {
        public GenericError()
        {
        }
        public GenericError(string message)
        {
            Message = message;
        }

        public string Message { get; set; }

        public override string ToString()
        {
            return "An error has occurred: " + Message;
        }
    }
}