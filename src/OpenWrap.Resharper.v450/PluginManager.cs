using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EnvDTE;
using EnvDTE80;
using OpenWrap.Commands.Cli;
using OpenWrap.VisualStudio;

namespace OpenWrap.Resharper
{
    public class PluginManager : MarshalByRefObject
    {
        List<Assembly> _loadedAssemblies = new List<Assembly>();
        const int MAX_RETRIES = 50;
        const int RETRY_DELAY = 2000;
        bool _resharperLoaded = false;

        public PluginManager()
        {
            OpenWrapOutput.Write("Resharper Plugin Manager loaded.");
            StartDetection();
        }

        void StartDetection()
        {
            
        }

        public void PluginAssembliesChanged(IEnumerable<string> assemblyPaths)
        {
            
        }

        public void Unload()
        {
            OpenWrapOutput.Write("Unloading Resharper Plugin Manager.");
        }
    }
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
                System.Diagnostics.Debugger.Launch();
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