using System;

namespace OpenWrap.Commands
{
    public class Error : GenericMessage
    {
        public const string ERROR_MESSAGE = "An unknown error has occured";

        public Error(string message, params object[] args) : base(message, args, ERROR_MESSAGE, CommandResultType.Error)
        {
        }

        public Error(string message) : base(message, null, ERROR_MESSAGE, CommandResultType.Error)
        {
        }

        protected Error() : base(null, null, null, CommandResultType.Error)
        {
        }
    }
}