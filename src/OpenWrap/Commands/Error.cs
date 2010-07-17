using System;
using OpenWrap.Commands;

namespace OpenWrap.Commands
{
    public abstract class Error : ICommandOutput
    {
        public Error()
        {
            Type = CommandResultType.Error;

        }
        public ICommand Source { get; set; }

        public CommandResultType Type { get; protected set; }

        public bool Success { get; private set; }
    }
}