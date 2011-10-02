extern alias resharper;
using ResharperSolutionComponentAttribute = resharper::JetBrains.ProjectModel.SolutionComponentImplementationAttribute;
namespace OpenWrap.Resharper
{
#if DEBUG
    [ResharperSolutionComponent]
    public class LegacyTestRunner :  resharper::JetBrains.ProjectModel.ISolutionComponent
    {
        TestRunner _runner;
        public LegacyTestRunner(resharper::JetBrains.ProjectModel.ISolution solution)
        {
            
        }

        public void Dispose()
        {
            _runner.Dispose();
        }

        public void Init()
        {
            var solution = resharper::JetBrains.ProjectModel.SolutionManager.Instance.CurrentSolution;
            _runner = new TestRunner(
                new LegacyShellThreading(),
                resharper::JetBrains.ActionManagement.ActionManager.Instance,
                resharper::JetBrains.ReSharper.Daemon.SolutionAnalysis.SolutionAnalysisManager.GetInstance(solution),
                solution
                );
        }

        public void AfterSolutionOpened()
        {
        }

        public void BeforeSolutionClosed()
        {
            _runner.Dispose();
            _runner = null;
        }

    }
#endif
}