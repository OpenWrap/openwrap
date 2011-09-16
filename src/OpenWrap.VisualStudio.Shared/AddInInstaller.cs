using System.IO;
using OpenWrap.VisualStudio.SolutionAddIn;

namespace OpenWrap.VisualStudio
{
    public static class AddInInstaller
    {
        static readonly string _addInPath = Path.Combine("openwrap", "VisualStudio");
        public static void Install(string assemblySource = null)
        {
            RegisterComAddIn<OpenWrapVisualStudioAddIn2008>(PerUserComComponentInstaller.ClrVersion2, assemblySource);
            RegisterComAddIn<OpenWrapVisualStudioAddIn2010>(PerUserComComponentInstaller.ClrVersion4, assemblySource);
        }
        public static void Uninstall()
        {
            UnregisterComAddIn<OpenWrapVisualStudioAddIn2008>();
            UnregisterComAddIn<OpenWrapVisualStudioAddIn2010>();
        }

        static void UnregisterComAddIn<T>(string assemblySource = null)
        {
            new PerUserComComponentInstaller<T>(_addInPath, assemblySource).Uninstall();
        }

        static void RegisterComAddIn<T>(string targetVersion, string assemblySource = null)
        {
            new PerUserComComponentInstaller<T>(_addInPath, assemblySource).Install(targetVersion);
        }
    }
}