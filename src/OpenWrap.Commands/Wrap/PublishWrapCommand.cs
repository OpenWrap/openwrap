using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenWrap.Commands.Wrap
{
    public class PublishWrapCommand : ICommand
    {
        [CommandInput(IsRequired = true, Position = 0)]
        public string RemoteRepository { get; set; }

        [CommandInput(IsRequired = true, Position = 1)]
        public string Name { get; set; }
        public IEnumerable<ICommandOutput> Execute()
        {
            yield break;
        }
    }
}
