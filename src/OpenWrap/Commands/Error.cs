using System;
using OpenWrap.Commands;

namespace OpenWrap.Commands
{
    public class Error : GenericMessage
    {
        public const string ERROR_MESSAGE = "An unknown error has occured";
        protected Error() : base(null, null, null, CommandResultType.Error){}
        public Error(string message, params object[] args) : base(message, args, ERROR_MESSAGE, CommandResultType.Error)
        {
        }

        public Error(string message) : base(message, null, ERROR_MESSAGE, CommandResultType.Error)
        {
        }
    }
    public class Warning : GenericMessage
    {
        public const string ERROR_MESSAGE = "An unknown warning has occured";
        protected Warning() : base(null, null, null, CommandResultType.Warning){}
        public Warning(string message, params object[] args)
            : base(message, args, ERROR_MESSAGE, CommandResultType.Warning)
        {
        }

        public Warning(string message)
            : base(message, null, ERROR_MESSAGE, CommandResultType.Warning)
        {
        }
    }
}