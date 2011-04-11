using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace OpenWrap.VisualStudio.Hooks
{
    public class SolutionAddIn
    {
        static bool called = false;
        static readonly object locker = new object();
        public static void Initialize()
        {
            lock (locker)
            {
                if (called) return;

                called = true;

                var dte = SiteManager.GetGlobalService<DTE>();
                if (dte == null)
                {
                    Debug.WriteLine("SolutionAddIn: DTE not found");
                    return;
                }
                dte.Solution.AddIns.Update();
                if (dte.Solution.AddIns.OfType<AddIn>().Any(x => x.ProgID == Constants.ADD_IN_PROGID)) return;

                AddInInstaller.RegisterAddInInUserHive();

                dte.Solution.AddIns.Add(Constants.ADD_IN_PROGID, Constants.ADD_IN_DESCRIPTION, Constants.ADD_IN_NAME, true);
            }
        }


    }
}
