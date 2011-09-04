using System.Linq;
using EnvDTE;
using EnvDTE80;
using OpenWrap.VisualStudio;

namespace OpenWrap.Resharper
{
    public  class OpenWrapOutput
    {
        readonly DTE2 _dte;
        OutputWindowPane _outputWindow;

        public OpenWrapOutput()
        {
            try
            {
                _dte = (DTE2)SiteManager.GetGlobalService<DTE>();
            }
            catch
            {
            }
        }

        public void Write(string text, params object[] args)
        {
            if (_dte == null) return;
            if (_outputWindow == null)
            {
                var output = (OutputWindow)_dte.Windows.Item(Constants.vsWindowKindOutput).Object;

                _outputWindow = output.OutputWindowPanes.Cast<OutputWindowPane>().FirstOrDefault(x => x.Name == "OpenWrap")
                                ?? output.OutputWindowPanes.Add("OpenWrap");
            }

            _outputWindow.OutputString(string.Format(text + "\r\n", args));
        }
    }
}