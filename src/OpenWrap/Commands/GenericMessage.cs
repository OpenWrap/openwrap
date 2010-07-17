using System;

namespace OpenWrap.Commands
{
    public class GenericMessage : ICommandOutput
    {
        public string Message { get; set; }

        public GenericMessage(string message)
        {
            Message = message;
            Type = CommandResultType.Default;
        }
        
        public bool Success
        {
            get { return true; }
        }

        public ICommand Source
        {
            get {return null; }
        }

        public CommandResultType Type { get; protected set; }

        public override string ToString()
        {
            return Message;
        }
    }
}