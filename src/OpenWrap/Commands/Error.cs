using System;
using OpenWrap.Commands;

namespace OpenWrap.Commands
{
    public abstract class Error : ICommandOutput
    {
        string _message;
        readonly object[] _messageArgs;

        public Error()
        {
            Type = CommandResultType.Error;
        }

        public Error(string message) : this()
        {
            _message = message;
        }

        public Error(string message, params object[] args) : this()
        {
            _message = message;
            _messageArgs = args;
        }

        public ICommand Source { get;  private set; }

        public CommandResultType Type { get; protected set; }

        public bool Success { get; private set; }
        public override string ToString()
        {
            return _messageArgs != null
                           ? string.Format(_message, _messageArgs)
                           : (_message
                             ?? "An unknown error has occured");
        }
    }
}