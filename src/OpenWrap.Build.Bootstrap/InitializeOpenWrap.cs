using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using OpenWrap.Preloading;

namespace OpenWrap.Build
{
    public class InitializeOpenWrap : Task
    {
        public string CurrentDirectory { get; set; }

        [Output]
        public string Name { get; set; }

        public bool StartDebug { get; set; }

        public override bool Execute()
        {
            if (StartDebug)
                Debugger.Launch();
            var asm = Preloader.LoadAssemblies(
                    Preloader.GetPackageFolders(Preloader.RemoteInstall.None, null, DefaultInstallationPaths.SystemRepositoryDirectory, "openwrap",  "openfilesystem", "sharpziplib")
                    );
            foreach (var assembly in asm.Select(x => x.Key))
                this.Log.LogMessage(MessageImportance.Low, "Pre-loaded assembly " + assembly);
            var openWrapAssembly = asm.Select(x=>x.Key).First(x => x.GetName().Name.Equals("openwrap", StringComparison.OrdinalIgnoreCase));
            var initializer = openWrapAssembly.GetType("OpenWrap.Build.BuildInitializer");
            var method = initializer.GetMethod("Initialize", BindingFlags.Static | BindingFlags.Public);
            Name = method.Invoke(null, new object[] { this.BuildEngine.ProjectFileOfTaskNode, CurrentDirectory }) as string;

            return true;
        }
    }
}