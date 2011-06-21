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
        DTE2 _dte;

        public OutputWindowCommandOutputFormatter()
        {
            try
            {
                _dte = (DTE2)SiteManager.GetGlobalService<DTE>();
            }
            catch
            {
                System.Diagnostics.Debugger.Launch();
            }
        }
        public void Render(ICommandOutput commandOutput)
        {
            if (_dte == null) return;
            if (_outputWindow == null)
            {
                var output = (OutputWindow)_dte.Windows.Item(EnvDTE.Constants.vsWindowKindOutput).Object;

                _outputWindow = output.OutputWindowPanes.Cast<OutputWindowPane>().FirstOrDefault(x => x.Name == "OpenWrap")
                    ?? output.OutputWindowPanes.Add("OpenWrap");
            }

            _outputWindow.OutputString(commandOutput + "\r\n");
        }
    }
}