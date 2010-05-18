using OpenWrap.Commands;

namespace OpenWrap.Commands
{
    public interface ICommandResult
    {
        bool Success { get; }
        ICommand Command { get; }
    }
}