using OpenRasta.Wrap.Commands;

namespace OpenRasta.Wrap.Console
{
    public interface ICommandResult
    {
        bool Success { get; }
        ICommand Command { get; }
    }
}