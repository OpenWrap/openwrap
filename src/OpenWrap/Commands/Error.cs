using OpenRasta.Wrap.Commands;

namespace OpenRasta.Wrap.Console
{
    public abstract class Error : ICommandResult
    {
        public ICommand Command { get; set; }
        public bool Success { get; private set; }
    }
}