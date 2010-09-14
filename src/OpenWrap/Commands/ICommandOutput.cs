using System.Collections.Generic;
using OpenWrap.Commands;

namespace OpenWrap.Commands
{
    public interface ICommandOutput
    {
        bool Success { get; }
        ICommand Source { get; }
        CommandResultType Type { get; }
    }
    public enum CommandResultType
    {
        Info = 0,
        Data,
        Verbose,
        Warning,
        Error
    }
}