using System.Collections.Generic;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;

namespace OpenWrap.VisualStudio.Hooks
{
    public static class ComExtensions
    {
        public static IEnumerable<CommandBarControl> Controls(this CommandBar commandBar)
        {
            if (commandBar.Controls.Count == 0) return Enumerable.Empty<CommandBarControl>();

            return commandBar.Controls.OfType<CommandBarControl>();
        }
        public static IEnumerable<CommandBarPopup> Popups(this CommandBar commandBar)
        {
            return commandBar.Controls().Where(_ => _.Type == MsoControlType.msoControlPopup).Cast<CommandBarPopup>();
        }
        public static CommandBar Named(this IEnumerable<CommandBarPopup> popups, string name)
        {
            return popups.Where(_ => _.CommandBar.Name == name).Select(_ => _.CommandBar).FirstOrDefault();
        }
        public static Command Named(this EnvDTE.Commands commands, string canonicalName)
        {

            Command testCommand = null;
            foreach (Command c in commands)
            {
                if (c.Name.Equals(canonicalName))
                    testCommand = c;
            }
            return testCommand;
        }
    }
}