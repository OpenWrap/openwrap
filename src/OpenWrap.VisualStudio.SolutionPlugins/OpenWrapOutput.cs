using System.Linq;
using EnvDTE;
using EnvDTE80;
using OpenWrap.VisualStudio;

namespace OpenWrap.SolutionPlugins.VisualStudio
{
    public static class OpenWrapOutput
    {
        static OutputWindowPane _outputWindow;
        static DTE2 _dte;

        static OpenWrapOutput()
        {
            try
            {
                _dte = (DTE2)SiteManager.GetGlobalService<DTE>();
            }
            catch
            {
            }
        }
        public static void Write(string text, params object[] args)
        {
            if (_dte == null) return;
            if (_outputWindow == null)
            {
                var output = (OutputWindow)_dte.Windows.Item(EnvDTE.Constants.vsWindowKindOutput).Object;

                _outputWindow = output.OutputWindowPanes.Cast<OutputWindowPane>().FirstOrDefault(x => x.Name == "OpenWrap")
                                ?? output.OutputWindowPanes.Add("OpenWrap");
            }

            _outputWindow.OutputString(string.Format(text + "\r\n", args));
        }
    }
}