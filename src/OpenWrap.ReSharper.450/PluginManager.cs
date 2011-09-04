extern alias resharper;
using System;
using System.Collections.Generic;
using System.Reflection;
using EnvDTE;
using EnvDTE80;
using OpenWrap.VisualStudio;

#if v600
using ResharperPluginManager = resharper::JetBrains.Application.PluginSupport.PluginManager;
using ResharperPlugin = resharper::JetBrains.Application.PluginSupport.Plugin;
using ResharperPluginTitleAttribute = resharper::JetBrains.Application.PluginSupport.PluginTitleAttribute;
using ResharperPluginDescriptionAttribute = resharper::JetBrains.Application.PluginSupport.PluginDescriptionAttribute;
using ResharperSolutionComponentAttribute = resharper::JetBrains.ProjectModel.SolutionComponentAttribute;
using ResharperAssemblyReference = resharper::JetBrains.ProjectModel.IProjectToAssemblyReference;
using ResharperThreading = resharper::JetBrains.Threading.IThreading;
#else
using ResharperPluginManager = resharper::JetBrains.UI.Application.PluginSupport.PluginManager;
using ResharperPlugin = resharper::JetBrains.UI.Application.PluginSupport.Plugin;
using ResharperPluginTitleAttribute = resharper::JetBrains.UI.Application.PluginSupport.PluginTitleAttribute;
using ResharperPluginDescriptionAttribute = resharper::JetBrains.UI.Application.PluginSupport.PluginDescriptionAttribute;
using ResharperThreading = OpenWrap.Resharper.IThreading;
using ResharperAssemblyReference = resharper::JetBrains.ProjectModel.IAssemblyReference;
#endif


[assembly: ResharperPluginTitleAttribute("OpenWrap ReSharper Integration")]


[assembly: ResharperPluginDescription("Provides integration of OpenWrap features within ReSharper.")]

namespace OpenWrap.Resharper
{
    public class PluginManager : MarshalByRefObject, IDisposable
    {
        public const string ACTION_REANALYZE = "ErrorsView.ReanalyzeFilesWithErrors";
        public const string OUTPUT_RESHARPER_TESTS = "OpenWrap-Tests";
        readonly DTE2 _dte;
        List<Assembly> _loadedAssemblies = new List<Assembly>();
        bool _resharperLoaded;

        static ResharperPlugin _selfPlugin;
        System.Threading.Thread _debugThread;
        bool runTestRunner = true;
        OpenWrapOutput _output;
        ResharperThreading _threading;

#if v600
        resharper::JetBrains.VsIntegration.Application.JetVisualStudioHost _host;
        resharper::JetBrains.Application.Parts.AssemblyPartsCatalogue _catalog;
        resharper::JetBrains.Application.PluginSupport.PluginsDirectory _pluginsDirectory;
#endif

        public const string RESHARPER_TEST = "?ReSharper";

        public PluginManager()
        {
            _dte = (DTE2)SiteManager.GetGlobalService<DTE>();
            _output = new OpenWrapOutput();
            _output.Write("Resharper Plugin Manager loaded ({0}).", GetType().Assembly.GetName().Version);

#if !v600
            _threading = new LegacyShellThreading();
#else       
            _host = resharper::JetBrains.VsIntegration.Application.JetVisualStudioHost.GetOrCreateHost((Microsoft.VisualStudio.OLE.Interop.IServiceProvider)_dte);
            var resolvedObj = _host.Environment.Container.ResolveDynamic(typeof(ResharperThreading));
            if (resolvedObj != null)
                _threading = (ResharperThreading)resolvedObj.Instance;
#endif

            _threading.Run("Loading plugins...", StartDetection);

        }

        public void Dispose()
        {
            _output.Write("Unloading Resharper Plugin Manager.");
            runTestRunner = false;
#if !v600
            _selfPlugin.Enabled = false;
            ResharperPluginManager.Instance.Plugins.Remove(_selfPlugin);
#else
            _pluginsDirectory.Plugins.Remove(_selfPlugin);

            _host = null;
            _catalog = null;
#endif
            _selfPlugin = null;
        }


        void StartDetection()
        {

            var asm = GetType().Assembly;
            var id = "ReSharper OpenWrap Integration";
#if v600
            _pluginsDirectory = (resharper::JetBrains.Application.PluginSupport.PluginsDirectory)_host.Environment.Container.ResolveDynamic(typeof(resharper::JetBrains.Application.PluginSupport.PluginsDirectory));
            _selfPlugin = new ResharperPlugin(null, new[] { new resharper::JetBrains.Util.FileSystemPath(asm.Location) }, null, null, null);
            _pluginsDirectory.Plugins.Add(_selfPlugin);

            
#else
            _selfPlugin = new ResharperPlugin(id, new[] { asm });

            ResharperPluginManager.Instance.Plugins.Add(_selfPlugin);
            _selfPlugin.Enabled = true;
            resharper::JetBrains.Application.Shell.Instance.LoadAssemblies(id, asm);
#endif
        }
    }


}