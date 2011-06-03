namespace OpenWrap.Commands
{
    public class Warning : AbstractOutput
    {
        public const string ERROR_MESSAGE = "An unknown warning has occured";

        public Warning(string message, params object[] args)
            : base(message ?? ERROR_MESSAGE, args)
        {
            Type = CommandResultType.Warning;
        }
    }
}