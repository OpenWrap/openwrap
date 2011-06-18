using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using EnvDTE;
using EnvDTE80;
using OpenFileSystem.IO;
using OpenWrap.Commands;
using OpenWrap.Reflection;
using OpenWrap.Resources;
using OpenWrap.Services;

namespace OpenWrap.VisualStudio.Resharper
{
    public class ResharperLoaderPlugin : IDisposable
    {
        readonly IFileSystem _fileSystem;
        const int WAIT_RETRY_MS = 2000;
        const int MAX_RETRIES = 50;
        int _retries = 0;
        static readonly IDictionary<Version, string> VersionToLoaders = new Dictionary<Version, string>
        {
            { new Version("4.5.1288.2"), "OpenWrap.Resharper.PluginManager, OpenWrap.Resharper.450" },
            { new Version("5.0.1659.36"), "OpenWrap.Resharper.PluginManager, OpenWrap.Resharper.500" },
            { new Version("5.1.1727.12"), "OpenWrap.Resharper.PluginManager, OpenWrap.Resharper.510" },
            { new Version("6.0.2162.902"), "OpenWrap.Resharper.PluginManager, OpenWrap.Resharper.600" }
        };

        Timer _timer;
        object _pluginManager;
        ITemporaryDirectory _assemblyDir;

        public ResharperLoaderPlugin() : this(ServiceLocator.GetService<IFileSystem>())
        {
        }

        public ResharperLoaderPlugin(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            _assemblyDir = _fileSystem.CreateTempDirectory();
            var vsAppDomain = AppDomain.CurrentDomain.GetData("openwrap.vs.appdomain") as AppDomain;
            
            if (vsAppDomain == null) return;

            DetectResharperLoading(vsAppDomain);
        }

        void DetectResharperLoading(AppDomain vsAppDomain)
        {
            Assembly resharperAssembly;
            try
            {
                resharperAssembly = vsAppDomain.Load("JetBrains.Platform.ReSharper.Shell");
            }
            catch
            {
                resharperAssembly = null;
            }
            if (resharperAssembly == null)
            {
                if (_retries++ <= MAX_RETRIES)
                {
                    OpenWrapOutput.Write("ReSharper not found, try {0}/{1}", _retries, MAX_RETRIES);
                    _timer = new Timer(_ => DetectResharperLoading(vsAppDomain), null, WAIT_RETRY_MS, Timeout.Infinite);
                }
                return;
            }
            var resharperVersion = resharperAssembly.GetName().Version;
            var pluginManagerType = LoadTypeFromVersion(resharperVersion);
            if (pluginManagerType == null) return;
            var sourceAssemblyFile = _fileSystem.GetFile(pluginManagerType.Assembly.Location);
            var destinationAssemblyFile = _assemblyDir.GetFile(sourceAssemblyFile.Name + ".dll");
            //var version = 
            sourceAssemblyFile.Sign(destinationAssemblyFile, new StrongNameKeyPair(Keys.openwrap));
            var plugin = vsAppDomain.CreateInstanceFromAndUnwrap(destinationAssemblyFile.Path, pluginManagerType.FullName);
            PluginLoaded(plugin);
        }

        void PluginLoaded(object plugin)
        {
            _pluginManager = plugin;
        }

        Type LoadTypeFromVersion(Version resharperAssembly)
        {
            var typeName = (from version in VersionToLoaders.Keys
                            orderby version descending
                            where resharperAssembly >= version
                            select VersionToLoaders[version]).FirstOrDefault();
            return typeName != null ? Type.GetType(typeName, false) : null;
        }

        public void Dispose()
        {
            _assemblyDir.Dispose();
        }

    }
    public static class OpenWrapOutput
    {
        static OutputWindowPane _outputWindow;
        static DTE2 _dte;

        static OpenWrapOutput()
        {
            try
            {
                _dte = (DTE2)SiteManager.GetGlobalService<DTE>();
            }
            catch
            {
            }
        }
        public static void Write(string text, params object[] args)
        {
            if (_dte == null) return;
            if (_outputWindow == null)
            {
                var output = (OutputWindow)_dte.Windows.Item(EnvDTE.Constants.vsWindowKindOutput).Object;

                _outputWindow = output.OutputWindowPanes.Cast<OutputWindowPane>().FirstOrDefault(x => x.Name == "OpenWrap")
                    ?? output.OutputWindowPanes.Add("OpenWrap");
            }

            _outputWindow.OutputString(string.Format(text + "\r\n", args));
        }
    }
}