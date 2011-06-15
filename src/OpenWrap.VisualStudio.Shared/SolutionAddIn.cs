using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using EnvDTE;
using OpenWrap.VisualStudio.SolutionAddIn;

namespace OpenWrap.VisualStudio.Hooks
{
    public class SolutionAddIn
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

                dte.Solution.AddIns.Update();

                if (dte.Solution.AddIns.OfType<EnvDTE.AddIn>().Any(x => x.ProgID == ComConstants.ADD_IN_PROGID_2010 || x.ProgID == ComConstants.ADD_IN_PROGID_2008))
                    return;

                AddInInstaller.Install();
                if (dte.Version == "9.0")
                    dte.Solution.AddIns.Add(ComConstants.ADD_IN_PROGID_2008, ComConstants.ADD_IN_DESCRIPTION, ComConstants.ADD_IN_NAME, true);
                else if (dte.Version == "10.0")
                    dte.Solution.AddIns.Add(ComConstants.ADD_IN_PROGID_2010, ComConstants.ADD_IN_DESCRIPTION, ComConstants.ADD_IN_NAME, true);
            }
        }
    }
}
