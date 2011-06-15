using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using EnvDTE;
using OpenWrap.VisualStudio.SolutionAddIn;

namespace OpenWrap.VisualStudio.Hooks
{
    public static class AddInInstaller
    {
        const string REG_INPROCSERVER32 = "InprocServer32";
        static readonly string _addInPath = Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "openwrap");
        public static void Install()
        {
            UnregisterComAddIn<OpenWrapVisualStudioAddIn2008>();
            UnregisterComAddIn<OpenWrapVisualStudioAddIn2010>();
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
        
        static string Combine(params string[] strings)
        {
            if (strings.Length == 0) return string.Empty;
            return strings[0] + strings.Skip(1).Aggregate(string.Empty, (i, str) => i + "\\" + str);
        }
    }

    public class PerUserComComponentInstaller<T> : PerUserComComponentInstaller
    {
        public PerUserComComponentInstaller(string path)
            : base(path, typeof(T))
        {
            _basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), path);
            VersionProvider = file => FileVersionInfo.GetVersionInfo(file).FileVersion.ToVersion();
            Type = typeof(T);
            ProgId = Type.Attribute<ProgIdAttribute>().Value;
            Guid = Type.Attribute<GuidAttribute>().Value;

        }
    }
}