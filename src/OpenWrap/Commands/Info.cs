namespace OpenWrap.Commands
{
    public class Info : AbstractOutput
    {
        public Info(string message = null, params object[] args) : base(message, args)
        {
            Type = CommandResultType.Info;
        }
    }
}