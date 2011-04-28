using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenWrap.Commands.Gui
{
    [Command(Verb="open", Noun="ide.gui", Visible = false)]
    public class ShowPackageExplorerCommand : AbstractCommand
    {
        protected override IEnumerable<ICommandOutput> ExecuteCore()
        {
            yield break;
        }
    }
}
