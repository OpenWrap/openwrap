using System;

namespace OpenWrap.Commands
{
    public class Error : AbstractOutput
    {
        const string ERROR_MESSAGE = "An unknown error has occured";

        public Error(string message = null, params object[] args) : base(message ?? ERROR_MESSAGE, args)
        {
            Type = CommandResultType.Error;
        }
    }
}