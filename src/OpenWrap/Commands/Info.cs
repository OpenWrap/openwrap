namespace OpenWrap.Commands
{
    public class Info : GenericMessage
    {
        public Info(string message, params object[] args) : base(message, args, string.Empty, CommandResultType.Info)
        {
        }
    }
}