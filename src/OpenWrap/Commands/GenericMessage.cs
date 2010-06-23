namespace OpenWrap.Commands
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
}