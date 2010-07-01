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

        public string Message { get; set; }

        public override string ToString()
        {
            return "An error has occurred: " + Message;
        }
    }
}