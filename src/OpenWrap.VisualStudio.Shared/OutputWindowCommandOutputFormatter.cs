using System.Linq;
using EnvDTE;
using EnvDTE80;
using OpenWrap.Commands;
using OpenWrap.Commands.Cli;

namespace OpenWrap.VisualStudio
{
    public class OutputWindowCommandOutputFormatter : ICommandOutputFormatter
    {
        OutputWindowPane _outputWindow;

        public void Render(ICommandOutput commandOutput)
        {
            DteGuard.BeginInvoke(dte =>
            {
                if (_outputWindow == null)
                {
                    var output = (OutputWindow)dte.Windows.Item(EnvDTE.Constants.vsWindowKindOutput).Object;

                    _outputWindow = output.OutputWindowPanes.Cast<OutputWindowPane>().FirstOrDefault(x => x.Name == "OpenWrap")
                                    ?? output.OutputWindowPanes.Add("OpenWrap");
                }

                _outputWindow.OutputString(commandOutput + "\r\n");
            });
        }
    }
}