using OpenWrap.Commands;

namespace OpenWrap.Commands
{
    public abstract class Error : ICommandResult
    {
        public ICommand Command { get; set; }
        public bool Success { get; private set; }
    }
}