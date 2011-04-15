using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Commands;

namespace Tests.Commands
{
    public class MemoryCommand : ICommand
    {
        public MemoryCommand()
        {
            Execute = ()=> Enumerable.Empty<ICommandOutput>();
        }

        public Func<IEnumerable<ICommandOutput>> Execute { get; set; }
        IEnumerable<ICommandOutput> ICommand.Execute()
        {
            return Execute();
        }
    }
}