using OpenRasta.Wrap.Commands;
using OpenRasta.Wrap.Console;

namespace OpenWrap.Commands.Wrap
{
    [Command(Name = "sync", Namespace = "wrap")]
    public class SyncWrapCommand : ICommand
    {
        public ICommandResult Execute()
        {
            return new Success();
        }
    }
}