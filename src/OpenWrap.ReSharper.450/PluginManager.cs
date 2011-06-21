extern alias resharper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using EnvDTE;
using EnvDTE80;
using OpenWrap.VisualStudio;
using ResharperPluginManager = resharper::JetBrains.UI.Application.PluginSupport.PluginManager;

[assembly: resharper::JetBrains.UI.Application.PluginSupport.PluginTitleAttribute("OpenWrap ReSharper Integration")]
[assembly: resharper::JetBrains.UI.Application.PluginSupport.PluginDescription("Provides integration of OpenWrap features within ReSharper.")]

namespace OpenWrap.Resharper
{
    public class PluginManager : MarshalByRefObject, IDisposable
    {
        public const string ACTION_REANALYZE = "ErrorsView.ReanalyzeFilesWithErrors";
        public const string OUTPUT_RESHARPER_TESTS = "OpenWrap-Tests";
        const int MAX_RETRIES = 50;
        const int RETRY_DELAY = 2000;
        readonly DTE2 _dte;
        List<Assembly> _loadedAssemblies = new List<Assembly>();
        bool _resharperLoaded;
        resharper::JetBrains.UI.Application.PluginSupport.Plugin _selfPlugin;
        System.Threading.Thread _debugThread;
        bool runTestRunner = true;
        public const string RESHARPER_TEST = "?ReSharper";

        public PluginManager()
        {
            _dte = (DTE2)SiteManager.GetGlobalService<DTE>();
            OpenWrapOutput.Write("Resharper Plugin Manager loaded ({0}).", GetType().Assembly.GetName().Version);

            Guard.Run("Loading plugins...", StartDetection);
        }

        public void Dispose()
        {
            OpenWrapOutput.Write("Unloading Resharper Plugin Manager.");
            runTestRunner = false;
            _selfPlugin.Enabled = false;
            ResharperPluginManager.Instance.Plugins.Remove(_selfPlugin);
            _selfPlugin = null;
        }


        void StartDetection()
        {

            var asm = GetType().Assembly;
            var id = "ReSharper OpenWrap Integration";
            _selfPlugin = new resharper::JetBrains.UI.Application.PluginSupport.Plugin(id, new[] { asm });

            ResharperPluginManager.Instance.Plugins.Add(_selfPlugin);
            _selfPlugin.Enabled = true;
            resharper::JetBrains.Application.Shell.Instance.LoadAssemblies(id, asm);
        }


    }
    [resharper::JetBrains.ProjectModel.SolutionComponentImplementation]
    public class TestRunner : resharper::JetBrains.ProjectModel.ISolutionComponent
    {
        bool runTestRunner = true;
        System.Threading.Thread _debugThread;
        DTE2 _dte;
        OutputWindowPane _pane;

        public void Dispose()
        {
        }

        public void Init()
        {
            
            _dte = (DTE2)SiteManager.GetGlobalService<DTE>();

            var output = (OutputWindow)_dte.Windows.Item(Constants.vsWindowKindOutput).Object;
            _pane = output.OutputWindowPanes.Add(PluginManager.OUTPUT_RESHARPER_TESTS);
            _debugThread = new System.Threading.Thread(WaitForOutput) { Name = "OpenWrap Test Runner Thread" };
            _debugThread.SetApartmentState(ApartmentState.STA);

        }

        void WaitForOutput()
        {
            OpenWrapOutput.Write("ReSharper test runner starting.");
            while (runTestRunner)
            {
                ProcessTest(_pane);
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }

        void ProcessTest(OutputWindowPane pane)
        {
            string text = null;
            Guard.Run("Reading pane",
                      () =>
                      {
                          text = pane.Read()
                                     .Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                                     .LastOrDefault(x => x.StartsWith(PluginManager.RESHARPER_TEST));
                      });
            if (text == null) return;
            var methodName = text.Substring(PluginManager.RESHARPER_TEST.Length);

            pane.Clear();
            Debug.WriteLine("Running test " + methodName);
            var runner = new ResharperTests(pane);
            var result = typeof(ResharperTests).GetMethod(methodName).Invoke(runner, new object[0]);

            var msg = string.Format("!ReSharper{0}:{1}\r\n", methodName, result);
            OpenWrapOutput.Write(msg);
            pane.OutputString(msg);
        }

        public void AfterSolutionOpened()
        {
            _debugThread.Start();
        }

        public void BeforeSolutionClosed()
        {
            runTestRunner = false;
            _debugThread.Join();
        }
    }

    public class ResharperTests
    {
        readonly OutputWindowPane _pane;

        public ResharperTests(OutputWindowPane pane)
        {
            _pane = pane;
        }

        public bool SolutionAnalysisHasNoErrors()
        {
            bool hasErrors = false;
            var solution = resharper::JetBrains.ProjectModel.SolutionManager.Instance.CurrentSolution;
            var sam = resharper::JetBrains.ReSharper.Daemon.SolutionAnalysis.SolutionAnalysisManager.GetInstance(solution);
            Guard.Run("TestRunner", ActivateSolutionAnalysis);
            resharper::JetBrains.Threading.JetDispatcher.Run(() => !sam.AnalysisComplete, TimeSpan.MaxValue, true);
            var sw = Stopwatch.StartNew();
            do
            {
                Guard.Run("Rerun failed tests",
                          () =>
                          {
#if v450
                              var action = resharper::JetBrains.ActionManagement.ActionManager.Instance.GetAction(PluginManager.ACTION_REANALYZE);
#else
                          var action = resharper::JetBrains.ActionManagement.ActionManager.Instance.GetUpdatableAction(PluginManager.ACTION_REANALYZE);
#endif
                              hasErrors = resharper::JetBrains.ActionManagement.ActionManager.Instance.UpdateAction(action);
                              if (hasErrors)
                              {
                                  sam.ReanalyzeAll();
                              }
                          });
                if (hasErrors)
                    resharper::JetBrains.Threading.JetDispatcher.Run(() => !sam.AnalysisComplete, TimeSpan.MaxValue, true);
                
            } while (hasErrors && CanContinueWaiting(sw, TimeSpan.FromSeconds(60)));
            return !hasErrors;
        }
        bool CanContinueWaiting(Stopwatch sw, TimeSpan time)
        {
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));
            return sw.Elapsed < time;
        }
        void ActivateSolutionAnalysis()
        {
            var solution = resharper::JetBrains.ProjectModel.SolutionManager.Instance.CurrentSolution;
            var sas = resharper::JetBrains.ReSharper.Daemon.Impl.SolutionAnalysisService.GetInstance(solution);
            sas.AnalysisEnabledOption = true;
        }
    }
    public static class DteExtensions
    {
        public static string Read(this OutputWindowPane pane)
        {
            if (pane == null) return null;

            var text = pane.TextDocument;
            text.Selection.StartOfDocument();
            text.Selection.EndOfDocument(true);
            return text.Selection.Text;
        }
    }

    public static class OpenWrapOutput
    {
        static readonly DTE2 _dte;
        static OutputWindowPane _outputWindow;

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
                var output = (OutputWindow)_dte.Windows.Item(Constants.vsWindowKindOutput).Object;

                _outputWindow = output.OutputWindowPanes.Cast<OutputWindowPane>().FirstOrDefault(x => x.Name == "OpenWrap")
                                ?? output.OutputWindowPanes.Add("OpenWrap");
            }

            _outputWindow.OutputString(string.Format(text + "\r\n", args));
        }
    }
}