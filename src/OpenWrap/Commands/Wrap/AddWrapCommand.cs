using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenRasta.Wrap.Commands;
using OpenRasta.Wrap.Console;

namespace OpenWrap.Commands.Wrap
{
    [Command(Name="add", Namespace="wrap")]
    public class AddWrapCommand : ICommand
    {
        [CommandInput(IsRequired=true, Position=0)]
        public string Name { get; set; }

        [CommandInput(Position=1)]
        public string Version { get; set; }

        public ICommandResult Execute()
        {
            return new Success();
        }
    }
}
