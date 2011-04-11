using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenWrap.Commands.Gui
{
    [Command(Verb="invoke", Noun="gui")]
    [UICommand(Label = "OpenWrap Dashboard", Context=UICommandContext.Global)]
    public class InvokeCommand : AbstractCommand
    {
        public override IEnumerable<ICommandOutput> Execute()
        {
            yield break;
        }
    }
}
