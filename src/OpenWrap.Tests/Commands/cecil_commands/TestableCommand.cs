using System.Collections.Generic;
using System.Linq;
using OpenWrap.Commands;

namespace Tests.Commands.cecil_commands
{
    public class TestableCommand : ICommand
    {
        public bool Executed;

        public IEnumerable<ICommandOutput> Execute()
        {
            Executed = true;
            return Enumerable.Empty<ICommandOutput>();
        }
    }
}