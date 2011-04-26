using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenWrap.Commands.Gui
{

    [Command(Verb = "invoke", Noun = "gui", Visible = false)]
    [UICommand(Label = "OpenWrap Dashboard", Context = UICommandContext.Global)]
    public class InvokeGuiCommand : AbstractCommand
    {
        protected override IEnumerable<ICommandOutput> ExecuteCore()
        {
            yield break;
        }
    }
}
