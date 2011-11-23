extern alias resharper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using EnvDTE;
using EnvDTE80;
using OpenWrap.VisualStudio;

#if v600
using ResharperPluginManager = resharper::JetBrains.Application.PluginSupport.PluginManager;
using ResharperPlugin = resharper::JetBrains.Application.PluginSupport.Plugin;
using ResharperPluginTitleAttribute = resharper::JetBrains.Application.PluginSupport.PluginTitleAttribute;
using ResharperPluginVendorAttribute = resharper::JetBrains.Application.PluginSupport.PluginVendorAttribute;
using ResharperPluginDescriptionAttribute = resharper::JetBrains.Application.PluginSupport.PluginDescriptionAttribute;
using ResharperSolutionComponentAttribute = resharper::JetBrains.ProjectModel.SolutionComponentAttribute;
using ResharperAssemblyReference = resharper::JetBrains.ProjectModel.IProjectToAssemblyReference;
using ResharperThreading = resharper::JetBrains.Threading.IThreading;
#else
using ResharperPluginManager = resharper::JetBrains.UI.Application.PluginSupport.PluginManager;
using ResharperPlugin = resharper::JetBrains.UI.Application.PluginSupport.Plugin;
using ResharperPluginTitleAttribute = resharper::JetBrains.UI.Application.PluginSupport.PluginTitleAttribute;
using ResharperPluginDescriptionAttribute = resharper::JetBrains.UI.Application.PluginSupport.PluginDescriptionAttribute;
using ResharperPluginVendorAttribute = resharper::JetBrains.UI.Application.PluginSupport.PluginVendorAttribute;
using ResharperThreading = OpenWrap.Resharper.IThreading;
using ResharperAssemblyReference = resharper::JetBrains.ProjectModel.IAssemblyReference;
#endif


[assembly: ResharperPluginTitleAttribute("OpenWrap ReSharper Integration")]
[assembly: ResharperPluginDescription("Provides integration of OpenWrap features within ReSharper.")]
[assembly: ResharperPluginVendorAttribute("Caffeine IT")]

namespace OpenWrap.Resharper
{
    /// <summary>
    /// Provides support for dynamic loading and unloading of ReSharper plugins, always lives in the same AppDomain as ReSharper.
    /// </summary>
    public class PluginManager : MarshalByRefObject, IDisposable
    {

        public const string ACTION_REANALYZE = "ErrorsView.ReanalyzeFilesWithErrors";
        public const string OUTPUT_RESHARPER_TESTS = "OpenWrap-Tests";
        readonly DTE2 _dte;
        List<Assembly> _loadedAssemblies = new List<Assembly>();
        bool _resharperLoaded;

        IEnumerable<ResharperPlugin> _currentPlugins = Enumerable.Empty<ResharperPlugin>();
        System.Threading.Thread _debugThread;
        bool runTestRunner = true;
        OpenWrapOutput _output;
        ResharperThreading _threading;
        System.Threading.Thread _monitor;
        ManualResetEvent _shutdownSync = new ManualResetEvent(false);
        EventWaitHandle _pluginsCanged;
        bool _shuttingDown;
#if v600
        resharper::JetBrains.VsIntegration.Application.JetVisualStudioHost _host;
        resharper::JetBrains.Application.Parts.AssemblyPartsCatalogue _catalog;
        resharper::JetBrains.Application.PluginSupport.PluginsDirectory _pluginsDirectory;
        resharper::JetBrains.DataFlow.LifetimeDefinition _lifetimeDefinition;
        
        
#endif

        public const string RESHARPER_TEST = "?ReSharper";

        public PluginManager()
        {
            _dte = (DTE2)SiteManager.GetGlobalService<DTE>();
            _output = new OpenWrapOutput("Resharper Plugin Manager");
            _output.Write("Loaded ({0}).", GetType().Assembly.GetName().Version);

#if !v600
            _threading = new LegacyShellThreading();
#else       
            _host = resharper::JetBrains.VsIntegration.Application.JetVisualStudioHost.GetOrCreateHost((Microsoft.VisualStudio.OLE.Interop.IServiceProvider)_dte);
            var resolvedObj = _host.Environment.Container.ResolveDynamic(typeof(ResharperThreading));
            if (resolvedObj != null)
                _threading = (ResharperThreading)resolvedObj.Instance;
#endif
            if (_threading == null)
            {
                _output.Write("Threading not found, the plugin manager will not initialize.");
                return;
            }
            _pluginsCanged = new EventWaitHandle(false, EventResetMode.AutoReset, System.Diagnostics.Process.GetCurrentProcess().Id + RESHARPER_ASSEMBLY_NOTIFY);
            _threading.Run("Loading plugins...", StartDetection);

        }

        public void Dispose()
        {
            _shuttingDown = true;
            _shutdownSync.Set();

            _output.Write("Unloading.");
            runTestRunner = false;
            foreach(var plugin in _currentPlugins)
                UnloadPlugin(plugin);
            _currentPlugins = Enumerable.Empty<ResharperPlugin>();
#if v600
            _lifetimeDefinition.Terminate();
            _host = null;
            _catalog = null;
#endif
        }
        void UnloadPlugin(ResharperPlugin plugin)
        {
#if !v600
            plugin.Enabled = false;
            ResharperPluginManager.Instance.Plugins.Remove(plugin);
#else
            plugin.IsEnabled.SetValue(false);
            _pluginsDirectory.Plugins.Remove(plugin);
#endif
        }
        public override object InitializeLifetimeService()
        {
            return null;
        }
        void StartDetection()
        {
            try
            {
                
#if v600
                _lifetimeDefinition = resharper::JetBrains.DataFlow.Lifetimes.Define(resharper::JetBrains.DataFlow.EternalLifetime.Instance, "OpenWrap Solution");
                _pluginsDirectory =
                    (resharper::JetBrains.Application.PluginSupport.PluginsDirectory)_host.Environment.Container.ResolveDynamic(typeof(resharper::JetBrains.Application.PluginSupport.PluginsDirectory)).Instance;
#endif
                _monitor = new System.Threading.Thread(Monitor);
                _monitor.Start();
            }
            catch (Exception e)
            {
                _output.Write("Plugin integration failed.\r\n" + e.ToString());
            }
        }
        void Monitor()
        {
            while (!_shuttingDown)
            {
                
                EventWaitHandle wait = null;
                try
                {
                    wait = EventWaitHandle.OpenExisting(System.Diagnostics.Process.GetCurrentProcess().Id + RESHARPER_ASSEMBLY_NOTIFY);
                }
                catch
                {
                }

                if (wait == null)
                {
                    System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));
                    continue;
                }

                WaitHandle.WaitAny(new WaitHandle[] { wait, _shutdownSync });
                if (_shuttingDown) return;
                
                _threading.Run("Updating plugins.", Refresh);
            }
        }
        public void Refresh()
        {
            if (_shuttingDown) return;
            foreach(var plugin in _currentPlugins)
                UnloadPlugin(plugin);
            var allPlugins = new[]{new List<string>{GetType().Assembly.Location}}
                .Concat(GetPlugins())
                .ToList();

            _currentPlugins = Load(allPlugins).ToList();
        }
        const string RESHARPER_PLUGIN_ASSEMBLIES = "RESHARPER_PLUGIN_ASSEMBLIES";
        const string RESHARPER_ASSEMBLY_NOTIFY = "RESHARPER_PLUGIN_CHANGE_NOTIFY";

        List<List<string>> GetPlugins()
        {
            return ((List<List<string>>)AppDomain.CurrentDomain.GetData(RESHARPER_PLUGIN_ASSEMBLIES)).Select(_ => _.ToList()).ToList();
        }

        IEnumerable<ResharperPlugin> Load(List<List<string>> plugins)
        {
            foreach(var plugin in plugins)
            {
#if v600
                var assemblies = 
                    plugin.Select(asm=>new resharper::JetBrains.Util.FileSystemPath(asm))
                          .ToArray();
                var pluginInstance =
                    new ResharperPlugin(_lifetimeDefinition.Lifetime, assemblies, null, null, null);

                _pluginsDirectory.Plugins.Add(pluginInstance);
#else

                var assemblies = plugin.Select(Assembly.LoadFrom).ToArray();
                var id = Guid.NewGuid().ToString();
                var pluginInstance = new ResharperPlugin(id, assemblies);

                ResharperPluginManager.Instance.Plugins.Add(pluginInstance);
                pluginInstance.Enabled = true;
                resharper::JetBrains.Application.Shell.Instance.LoadAssemblies(id, assemblies);
                
#endif
                yield return pluginInstance;
            }
        }
    }


}