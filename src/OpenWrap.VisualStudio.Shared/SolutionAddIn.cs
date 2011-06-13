using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using EnvDTE;

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

                if (dte.Solution.AddIns.OfType<AddIn>().Any(x => x.ProgID == Constants.ADD_IN_PROGID_2010 || x.ProgID == Constants.ADD_IN_PROGID_2008))
                    return;

                AddInInstaller.RegisterAddInInUserHive();
                if (dte.Version == "9.0")
                    dte.Solution.AddIns.Add(Constants.ADD_IN_PROGID_2008, Constants.ADD_IN_DESCRIPTION, Constants.ADD_IN_NAME, true);
                else if (dte.Version == "10.0")
                    dte.Solution.AddIns.Add(Constants.ADD_IN_PROGID_2010, Constants.ADD_IN_DESCRIPTION, Constants.ADD_IN_NAME, true);
            }
        }
    }
}
