using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenWrap.Services;

namespace OpenWrap.Commands.Wrap
{
    public class PublishWrapCommand : ICommand
    {
        [CommandInput(IsRequired = true, Position = 0)]
        public string RemoteRepository { get; set; }

        [CommandInput(IsRequired = true, Position = 1)]
        public string Path { get; set; }

        
        public IEnumerable<ICommandOutput> Execute()
        {

            yield break;
        }
    }
}
