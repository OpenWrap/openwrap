using OpenWrap.Commands;

namespace OpenWrap.Commands.Wrap
{
    public class GenericError : Error
    {
        public string Message { get; set; }

        public override string ToString()
        {
            return "An error has occurred: " + Message;
        }
    }
}