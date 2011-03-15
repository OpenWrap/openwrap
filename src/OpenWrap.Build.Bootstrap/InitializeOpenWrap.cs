using System;
using System.Collections.Generic;
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

        [Output]
        public string Scope { get; set; }

        [Output]
        public string DescriptorPath { get; set; }

        public string CurrentProjectFile { get; set; }

        public bool StartDebug { get; set; }

        public override bool Execute()
        {
            if (StartDebug)
                Debugger.Launch();
            var asm = Preloader.LoadAssemblies(
                    Preloader.GetPackageFolders(Preloader.RemoteInstall.None, Environment.CurrentDirectory, DefaultInstallationPaths.SystemRepositoryDirectory,"openwrap", "openfilesystem", "sharpziplib")
                    );
            foreach (var assembly in asm.Select(x => x.Key))
                Log.LogMessage(MessageImportance.Low, "Pre-loaded assembly " + assembly);
            var openWrapAssembly = asm.Select(x=>x.Key).First(x => x.GetName().Name.Equals("openwrap", StringComparison.OrdinalIgnoreCase));
            var initializer = openWrapAssembly.GetType("OpenWrap.Build.BuildInitializer");
            var method = initializer.GetMethod("Initialize", BindingFlags.Static | BindingFlags.Public);
            var values = method.Invoke(null, new object[] { CurrentProjectFile, CurrentDirectory }) as IDictionary<string,string>;
            Name = values[BuildConstants.PACKAGE_NAME];
            Scope = values[BuildConstants.PROJECT_SCOPE];
            DescriptorPath = values[BuildConstants.DESCRIPTOR_PATH];

            Log.LogMessage(MessageImportance.Normal, "OpenWrap correctly initialized.");
            foreach(var kv in values)
                Log.LogMessage(MessageImportance.Normal, "{0}: {1}", kv.Key, kv.Value);

            return true;
        }
    }
}