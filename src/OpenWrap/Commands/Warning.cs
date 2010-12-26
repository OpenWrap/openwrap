namespace OpenWrap.Commands
{
    public class Warning : GenericMessage
    {
        public const string ERROR_MESSAGE = "An unknown warning has occured";

        public Warning(string message, params object[] args)
                : base(message, args, ERROR_MESSAGE, CommandResultType.Warning)
        {
        }

        public Warning(string message)
                : base(message, null, ERROR_MESSAGE, CommandResultType.Warning)
        {
        }

        protected Warning() : base(null, null, null, CommandResultType.Warning)
        {
        }
    }
}