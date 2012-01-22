extern alias resharper;
using System;
using System.Diagnostics;
using EnvDTE;

#if v600 || v610
using ResharperPluginManager = resharper::JetBrains.Application.PluginSupport.PluginManager;
using ResharperPlugin = resharper::JetBrains.Application.PluginSupport.Plugin;
using ResharperPluginTitleAttribute = resharper::JetBrains.Application.PluginSupport.PluginTitleAttribute;
using ResharperPluginDescriptionAttribute = resharper::JetBrains.Application.PluginSupport.PluginDescriptionAttribute;
using ResharperSolutionComponentAttribute = resharper::JetBrains.ProjectModel.SolutionComponentAttribute;
using ResharperThreading = resharper::JetBrains.Threading.IThreading;
using ResharperUpdatableAction = resharper::JetBrains.ActionManagement.IUpdatableAction;
#else
using ResharperPluginManager = resharper::JetBrains.UI.Application.PluginSupport.PluginManager;
using ResharperPlugin = resharper::JetBrains.UI.Application.PluginSupport.Plugin;
using ResharperPluginTitleAttribute = resharper::JetBrains.UI.Application.PluginSupport.PluginTitleAttribute;
using ResharperPluginDescriptionAttribute = resharper::JetBrains.UI.Application.PluginSupport.PluginDescriptionAttribute;
using ResharperSolutionComponentAttribute = resharper::JetBrains.ProjectModel.SolutionComponentImplementationAttribute;
using ResharperThreading = OpenWrap.Resharper.IThreading;
using ResharperUpdatableAction = resharper::JetBrains.ActionManagement.IUpdatableAction;
#endif

namespace OpenWrap.Resharper
{
#if DEBUG
    public class ResharperTests : IResharperTests
    {
        readonly OutputWindowPane _pane;
        readonly ResharperThreading _threading;
        readonly resharper::JetBrains.ActionManagement.ActionManager _actionManager;
        readonly resharper::JetBrains.ReSharper.Daemon.SolutionAnalysis.SolutionAnalysisManager _solutionManager;
        readonly resharper::JetBrains.ProjectModel.ISolution _solution;

        public ResharperTests(OutputWindowPane pane,
            ResharperThreading threading,
            resharper::JetBrains.ActionManagement.ActionManager actionManager,
            resharper::JetBrains.ReSharper.Daemon.SolutionAnalysis.SolutionAnalysisManager solutionManager,
            resharper::JetBrains.ProjectModel.ISolution solution)
        {
            _pane = pane;
            _threading = threading;
            _actionManager = actionManager;
            _solutionManager = solutionManager;
            _solution = solution;
        }

        public bool SolutionAnalysisHasNoErrors()
        {
            bool hasErrors = false;
            _threading.Run("TestRunner", ActivateSolutionAnalysis);
            resharper::JetBrains.Threading.JetDispatcher.Run(() => !_solutionManager.AnalysisComplete, TimeSpan.MaxValue, true);
            var sw = Stopwatch.StartNew();
            do
            {
                _threading.Run("Rerun failed tests",
                          () =>
                          {
#if v450
                              var action = _actionManager.GetAction(PluginManager.ACTION_REANALYZE);
#elif !v600 && !v610
                              var action = _actionManager.GetUpdatableAction(PluginManager.ACTION_REANALYZE);
#else
                              var action = _actionManager.TryGetAction(PluginManager.ACTION_REANALYZE);
#endif
                              
                              hasErrors = _actionManager.UpdateAction((ResharperUpdatableAction)action);
                              if (hasErrors)
                              {
                                  _solutionManager.ReanalyzeAll();
                              }
                          });
                if (hasErrors)
                    resharper::JetBrains.Threading.JetDispatcher.Run(() => !_solutionManager.AnalysisComplete, TimeSpan.MaxValue, true);
                
            } while (hasErrors && CanContinueWaiting(sw, TimeSpan.FromSeconds(60)));
            return !hasErrors;
        }

        static bool CanContinueWaiting(Stopwatch sw, TimeSpan time)
        {
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));
            return sw.Elapsed < time;
        }

        void ActivateSolutionAnalysis()
        {
#if v450 || v500 || v510 || v600
            var sas = resharper::JetBrains.ReSharper.Daemon.Impl.SolutionAnalysisService.GetInstance(_solution);
            sas.AnalysisEnabledOption = true;
#elif v610
			resharper::JetBrains.Application.Shell.Instance.Invocator.ReentrancyGuard.Execute("SWEA", () =>
			{
			    var solutionDataContext = resharper::JetBrains.ProjectModel.DataContext.DataContextsEx.ToDataContext(_solution);
			    var settingsStore = resharper::JetBrains.Application.Shell.Instance.GetComponent<resharper::JetBrains.Application.Settings.Store.Implementation.SettingsStore>();
#pragma warning disable 612,618
                resharper::JetBrains.Application.Settings.SettingsStoreEx.SetValue(
                    settingsStore,
                    solutionDataContext, 
                    resharper::JetBrains.ReSharper.Daemon.HighlightingSettingsAccessor.AnalysisEnabled,
                    resharper::JetBrains.ReSharper.Daemon.AnalysisScope.SOLUTION);
			});
#pragma warning restore 612,618

#endif
        }
    }
#endif
}