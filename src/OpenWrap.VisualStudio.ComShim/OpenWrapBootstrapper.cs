using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using EnvDTE;
using EnvDTE80;
using Extensibility;
using Microsoft.Build.BuildEngine;
using Debugger = System.Diagnostics.Debugger;
using Project = EnvDTE.Project;

namespace OpenWrap.VisualStudio.ComShim
{
    [Guid(Constants.ADD_IN_GUID)]
    [ProgId(Constants.ADD_IN_PROGID)]
    [ComVisible(true)]
    public class OpenWrapBootstrapper : IDTExtensibility2
    {

        protected AddIn AddIn { get; set; }
        protected DTE2 Application { get; set; }

        public void OnAddInsUpdate(ref Array custom)
        {
        }

        public void OnBeginShutdown(ref Array custom)
        {
        }

        public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
        {
            Debugger.Break();
            Debug.WriteLine(string.Format("OpenWrapBootstrapper: Connecting solution add-in, .net version {0}", Environment.Version));
            var dte = application as DTE;
            if (dte == null) return;
            var engine = Engine(dte);
            if (engine == null) return;

            foreach (var project in from dteProject in dte.Solution.Projects.OfType<Project>()
                                    select engine.Load(dteProject.FullName))
                project.RunTarget("OpenWrap-Initialize");
        }

        public void OnDisconnection(ext_DisconnectMode removeMode, ref Array custom)
        {
        }

        public void OnStartupComplete(ref Array custom)
        {
        }
        IMSBuildEngine Engine(DTE dte)
        {
            if (dte.Version == "9.0")
                return new MSBuild3Engine();
            if (dte.Version == "10.0")
                return new MSBuild4Engine();
            return null;
        }

    }
;
}
