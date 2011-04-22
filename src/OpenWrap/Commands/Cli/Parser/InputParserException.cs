using System;

namespace OpenWrap.Commands.Cli.Parser
{
    public class InputParserException : Exception
    {
        public InputParserException(string message) : base(message)
        {
        }
    }
}