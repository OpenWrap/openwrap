using System.Diagnostics;
using EnvDTE;
using OpenWrap.VisualStudio.ProjectModel;

namespace OpenWrap.VisualStudio
{
    public class SolutionAddInEnabler
    {
        static bool _called;
        static readonly object LOCKER = new object();
        public static void Initialize()
        {
            lock (LOCKER)
            {
                if (_called) return;

                _called = true;

                var dte = SiteManager.GetGlobalService<DTE>();
                
                if (dte == null)
                {
                    Debug.WriteLine("SolutionAddIn: DTE not found");
                    return;
                }
                AddInInstaller.Install();

                var solution = new DteSolution(dte.Solution);
                solution.OpenWrapAddInEnabled = true;
            }
        }
    }
}
