extern alias resharper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EnvDTE;
using EnvDTE80;
using OpenWrap.Commands.Cli;
using OpenWrap.VisualStudio;
using ISolutionComponent = resharper::JetBrains.ProjectModel.ISolutionComponent;
using ResharperPluginManager = resharper::JetBrains.UI.Application.PluginSupport.PluginManager;

[assembly: resharper::JetBrains.UI.Application.PluginSupport.PluginTitleAttribute("OpenWrap ReSharper Integration")]
[assembly: resharper::JetBrains.UI.Application.PluginSupport.PluginDescription("Provides integration of OpenWrap features within ReSharper.")]
namespace OpenWrap.Resharper
{
    public class PluginManager : MarshalByRefObject
    {
        List<Assembly> _loadedAssemblies = new List<Assembly>();
        const int MAX_RETRIES = 50;
        const int RETRY_DELAY = 2000;
        bool _resharperLoaded = false;
        resharper::JetBrains.UI.Application.PluginSupport.Plugin _selfPlugin;

        public PluginManager()
        {
            OpenWrapOutput.Write("Resharper Plugin Manager loaded ({0}).", GetType().FullName);
            
            ResharperLocks.WriteCookie("Loading plugins...", StartDetection);
        }

        void StartDetection()
        {
            var asm = GetType().Assembly;
            //resharper::JetBrains.Application.Shell.Instance.LoadAssemblies(GetType().Assembly);
            _selfPlugin = new resharper::JetBrains.UI.Application.PluginSupport.Plugin("ReSharper OpenWrap Integration", new[] { asm });
            ResharperPluginManager.Instance.Plugins.Add(_selfPlugin);
            _selfPlugin.Enabled = true;
        }

        public void PluginAssembliesChanged(IEnumerable<string> assemblyPaths)
        {
            
        }

        public void Unload()
        {
            OpenWrapOutput.Write("Unloading Resharper Plugin Manager.");
            _selfPlugin.Enabled = false;
            ResharperPluginManager.Instance.Plugins.Remove(_selfPlugin);
            _selfPlugin = null;
        }
    }

    [resharper::JetBrains.ProjectModel.SolutionComponentImplementation]
    public class ProjectReferenceUpdater : ISolutionComponent
    {
        readonly resharper::JetBrains.ProjectModel.ISolution _solution;

        public ProjectReferenceUpdater(resharper::JetBrains.ProjectModel.ISolution solution)
        {
            _solution = solution;
            OpenWrapOutput.Write("Solution opened " + solution.Name);

        }

        public void Dispose()
        {
            OpenWrapOutput.Write("Dispose()");
            
        }

        public void Init()
        {
            OpenWrapOutput.Write("Init()");
        }

        public void AfterSolutionOpened()
        {
            OpenWrapOutput.Write("AfterSolutionOpened()");
        }

        public void BeforeSolutionClosed()
        {
            OpenWrapOutput.Write("BeforeSOlutionCLosed()");
        }
    }
    public static class OpenWrapOutput
    {
        static OutputWindowPane _outputWindow;
        static readonly DTE2 _dte;

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
                var output = (EnvDTE.OutputWindow)_dte.Windows.Item(EnvDTE.Constants.vsWindowKindOutput).Object;

                _outputWindow = output.OutputWindowPanes.Cast<OutputWindowPane>().FirstOrDefault(x => x.Name == "OpenWrap")
                    ?? output.OutputWindowPanes.Add("OpenWrap");
            }

            _outputWindow.OutputString(string.Format(text + "\r\n", args));
        }
    }
}