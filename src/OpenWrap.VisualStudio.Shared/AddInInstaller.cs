using System.IO;
using OpenWrap.VisualStudio.SolutionAddIn;

namespace OpenWrap.VisualStudio
{
    public static class AddInInstaller
    {
        static readonly string _addInPath = Path.Combine("openwrap", "VisualStudio");
        public static void Install()
        {
            // TODO: This prevents upgrading in scenarios where the assembly is already loaded <sigh>
            RegisterComAddIn<OpenWrapVisualStudioAddIn2008>(PerUserComComponentInstaller.ClrVersion2);
            RegisterComAddIn<OpenWrapVisualStudioAddIn2010>(PerUserComComponentInstaller.ClrVersion4);
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
}