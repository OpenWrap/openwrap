using System.Collections.Generic;

namespace OpenWrap.Commands.Gui
{
    [Command(Verb = "open", Noun = "gui")]
    [UICommand(Label = "OpenWrap Dashboard", Context = UICommandContext.Global)]
    public class InvokeGuiCommand : AbstractCommand
    {
        protected override IEnumerable<ICommandOutput> ExecuteCore()
        {
            yield break;
        }
    }
}