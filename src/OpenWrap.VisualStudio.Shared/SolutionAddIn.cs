using System.Diagnostics;
using EnvDTE;
using OpenWrap.VisualStudio.ProjectModel;

namespace OpenWrap.VisualStudio
{
    public class SolutionAddInEnabler
    {
        static bool _called;
        static readonly object LOCKER = new object();
        public static string Initialize()
        {
            lock (LOCKER)
            {
                if (_called) return null;

                _called = true;

                var dte = SiteManager.GetGlobalService<DTE>();
                
                AddInInstaller.Install();

                var solution = new DteSolution(dte.Solution);
                solution.OpenWrapAddInEnabled = true;
                return "OpenWrap enabled.";
            }
        }
    }
}
