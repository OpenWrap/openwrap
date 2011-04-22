using System.Collections.Generic;
using System.Linq;
using OpenWrap.Commands;

namespace Tests.Commands.cecil_commands
{
    [Command(Noun = "evil", Verb="be")]
    public class CommandWithoutInput : ICommand
    {
        public bool Executed = false;
        public IEnumerable<ICommandOutput> Execute()
        {
            Executed = true;
            return Enumerable.Empty<ICommandOutput>();
        }
    }
}