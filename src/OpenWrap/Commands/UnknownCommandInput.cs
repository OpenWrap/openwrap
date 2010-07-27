namespace OpenWrap.Commands
{
    public class UnknownCommandInput : Error
    {
        public UnknownCommandInput(string inputName) : base("Command input '{0}' wasn't recognized", inputName)
        {
        }

    }
}