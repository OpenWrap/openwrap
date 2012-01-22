extern alias resharper;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using EnvDTE;
using EnvDTE80;
using OpenWrap.VisualStudio;

#if v600 || v610
using ResharperPluginManager = resharper::JetBrains.Application.PluginSupport.PluginManager;
using ResharperPlugin = resharper::JetBrains.Application.PluginSupport.Plugin;
using ResharperPluginTitleAttribute = resharper::JetBrains.Application.PluginSupport.PluginTitleAttribute;
using ResharperPluginDescriptionAttribute = resharper::JetBrains.Application.PluginSupport.PluginDescriptionAttribute;
using ResharperSolutionComponentAttribute = resharper::JetBrains.ProjectModel.SolutionComponentAttribute;
using ResharperAssemblyReference = resharper::JetBrains.ProjectModel.IProjectToAssemblyReference;
using ResharperThreading = resharper::JetBrains.Threading.IThreading;
#else
using ResharperPluginManager = resharper::JetBrains.UI.Application.PluginSupport.PluginManager;
using ResharperPlugin = resharper::JetBrains.UI.Application.PluginSupport.Plugin;
using ResharperPluginTitleAttribute = resharper::JetBrains.UI.Application.PluginSupport.PluginTitleAttribute;
using ResharperPluginDescriptionAttribute = resharper::JetBrains.UI.Application.PluginSupport.PluginDescriptionAttribute;
using ResharperThreading = OpenWrap.Resharper.IThreading;
using ResharperAssemblyReference = resharper::JetBrains.ProjectModel.IAssemblyReference;
#endif
namespace OpenWrap.Resharper
{
#if DEBUG
#if v600 || v610
	[ResharperSolutionComponent]
#endif
    public class TestRunner : IDisposable
    {
        readonly ResharperThreading _threading;
        readonly resharper::JetBrains.ActionManagement.ActionManager _actionManager;
        readonly resharper::JetBrains.ReSharper.Daemon.SolutionAnalysis.SolutionAnalysisManager _saManager;
        bool _runTestRunner = true;
        System.Threading.Thread _debugThread;
        DTE2 _dte;
        OutputWindowPane _pane;
        OpenWrapOutput _output;
        ManualResetEvent _unloading = new ManualResetEvent(false);
        resharper::JetBrains.ProjectModel.ISolution _solution;

        public void Dispose()
        {   
            _unloading.Set();
            _runTestRunner = false;
            if (!_debugThread.Join(TimeSpan.FromSeconds(10)))
            {
                _output.Write("ReSharper TestRunner hasn't stopped on time, killing.");
                _debugThread.Abort();
            }
        }

        public TestRunner(ResharperThreading threading,
                          resharper::JetBrains.ActionManagement.ActionManager actionManager,
                          resharper::JetBrains.ReSharper.Daemon.SolutionAnalysis.SolutionAnalysisManager saManager,
                          resharper::JetBrains.ProjectModel.ISolution solution)
        {
            _threading = threading;
            _actionManager = actionManager;
            _saManager = saManager;

            _dte = (DTE2)SiteManager.GetGlobalService<DTE>();
            _output = new OpenWrapOutput();
            var output = (OutputWindow)_dte.Windows.Item(Constants.vsWindowKindOutput).Object;
            _pane = output.OutputWindowPanes.Add(PluginManager.OUTPUT_RESHARPER_TESTS);
            _debugThread = new System.Threading.Thread(WaitForOutput) { Name = "OpenWrap Test Runner Thread" };
            _debugThread.SetApartmentState(ApartmentState.STA);
            _debugThread.Start();
            _solution = solution;
        }

        void WaitForOutput()
        {
            _output.Write("ReSharper test runner starting.");
            while (_runTestRunner)
            {
                ProcessTest(_pane);
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }

        void ProcessTest(OutputWindowPane pane)
        {
            string paneText;
            var success = _threading.BeginInvokeAndWait("Reading pane",()=> ReadTest(pane), out paneText, _unloading);
            if (!success) return;

            if (paneText == null || paneText.Length <= PluginManager.RESHARPER_TEST.Length) return;
            var methodName = paneText.Substring(PluginManager.RESHARPER_TEST.Length);

            pane.Clear();
            Debug.WriteLine("Running test " + methodName);
            var runner = new ResharperTests(pane, _threading, _actionManager, _saManager, _solution);
            var result = typeof(ResharperTests).GetMethod(methodName).Invoke(runner, new object[0]);

            var msg = string.Format("!ReSharper{0}:{1}\r\n", methodName, result);
            _output.Write(msg);
            pane.OutputString(msg);
        }

        string ReadTest(OutputWindowPane pane)
        {
            return pane.Read().Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                .LastOrDefault(x => x.StartsWith(PluginManager.RESHARPER_TEST));
        }

    }
#endif
}