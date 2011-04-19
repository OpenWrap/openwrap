using System;

namespace OpenWrap.Commands.Cli
{
    public class InputParserException : Exception
    {
        public InputParserException(string message) : base(message)
        {
        }
    }
}