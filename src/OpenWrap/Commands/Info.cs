namespace OpenWrap.Commands
{
    public class Info : AbstractOutput
    {
        public Info(string message, params object[] args) : base(message, args)
        {
            Type = CommandResultType.Info;
        }
    }
}