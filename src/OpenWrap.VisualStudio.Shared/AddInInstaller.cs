using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using EnvDTE;
using OpenWrap.VisualStudio.SolutionAddIn;

namespace OpenWrap.VisualStudio.Hooks
{
    public static class AddInInstaller
    {
        const string REG_INPROCSERVER32 = "InprocServer32";
        static readonly string _addInPath = Path.Combine("openwrap", "VisualStudio");
        public static void Install()
        {
            RegisterComAddIn<OpenWrapVisualStudioAddIn2008>("v2.0.50727");
            RegisterComAddIn<OpenWrapVisualStudioAddIn2010>("v4.0.30319");
        }
        public static void Uninstall()
        {
            UnregisterComAddIn<OpenWrapVisualStudioAddIn2008>();
            UnregisterComAddIn<OpenWrapVisualStudioAddIn2010>();
        }

        static void UnregisterComAddIn<T>()
        {
            new PerUserComComponentInstaller<T>(_addInPath).Uninstall();
        }

        static void RegisterComAddIn<T>(string targetVersion)
        {
            new PerUserComComponentInstaller<T>(_addInPath).Install(targetVersion);
        }
    }

    public class PerUserComComponentInstaller<T> : PerUserComComponentInstaller
    {
        public PerUserComComponentInstaller(string path)
            : base(path, typeof(T))
        {
        }
    }
}