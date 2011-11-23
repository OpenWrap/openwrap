using System;
using System.Linq;
using System.Threading;
using EnvDTE;
using OpenFileSystem.IO;
using OpenWrap.IO;
using OpenWrap.PackageManagement;
using OpenWrap.PackageManagement.Exporters.Assemblies;
using OpenWrap.PackageModel.Serialization;
using OpenWrap.Repositories;
using OpenWrap.Runtime;
using OpenWrap.Services;
using OpenWrap.VisualStudio;
using OpenWrap.VisualStudio.ProjectModel;

namespace OpenWrap.SolutionPlugins.VisualStudio.ReSharper
{
    public class ReSharperPluginsChangePlugin : IDisposable
    {
        const string RESHARPER_PLUGIN_ASSEMBLIES = "RESHARPER_PLUGIN_ASSEMBLIES";
        const string RESHARPER_ASSEMBLY_NOTIFY = "RESHARPER_PLUGIN_CHANGE_NOTIFY";
        const int MAX_TRIES = 100;
        int _initTries = 0;
        EventWaitHandle _assembliesChanged;
        IPackageManager _packageManager;
        IPackageRepository _projectRepository;
        IDirectory _rootDirectory;
        Timer _timer;
        IDisposable _changeMonitor;
        bool _running;
        DTE _dte;
        int _retries;
        IEnvironment _environment;
        const int MAX_RETRIES = 20;

        public ReSharperPluginsChangePlugin()
        {
            if (!TryInitialize())
            {
                ScheduleTryInitialize();
            }
        }
        void ScheduleTryInitialize()
        {
            if (_initTries++ < MAX_TRIES)
                _timer = new Timer(_ =>
                {
                    if (!TryInitialize())
                        ScheduleTryInitialize();
                },
                                   null,
                                   TimeSpan.FromSeconds(2),
                                   TimeSpan.FromMilliseconds(-1));
        }
        bool TryInitialize()
        {
            try
            {
                _dte = SiteManager.GetGlobalService<DTE>();
                _running = _dte != null;
            }
            catch
            {
                _running = false;
            }
            if (!_running) return false;
            // TODO:  seen in the wild, _dte.Solution is null (?), need to schedule and restart initialization for those scenarios.
            _environment = ServiceLocator.GetService<IEnvironment>();
            _packageManager = ServiceLocator.GetService<IPackageManager>();

            _assembliesChanged = new EventWaitHandle(false, EventResetMode.AutoReset, System.Diagnostics.Process.GetCurrentProcess().Id + RESHARPER_ASSEMBLY_NOTIFY);
            _rootDirectory = _environment.DescriptorFile.Parent;
            _projectRepository = _environment.ProjectRepository;
            RegisterFileListener();
            RefreshPluginList();
            _timer = new Timer(_ => RefreshPluginList(), null, Timeout.Infinite, Timeout.Infinite);
            return true;
        }

        public void Dispose()
        {
            if (!_running) return;
            _assembliesChanged.Close();
            _changeMonitor.Dispose();
            _running = false;
            _dte = null;
        }

        void RefreshPluginList()
        {
            var descriptor = new PackageDescriptorReader().ReadAll(_rootDirectory)[string.Empty].Value;

            var vsAppDomain = (AppDomain)AppDomain.CurrentDomain.GetData("openwrap.vs.appdomain");
            var resharperVersion = DetectResharperLoading(vsAppDomain);
            var exporter = new DefaultAssemblyExporter("resharper" + resharperVersion);

            var plugins = _packageManager
                .ListProjectPackages(descriptor, _projectRepository)
                .Select(pack => exporter.Items<Exports.IAssembly>(pack.Load(), _environment.ExecutionEnvironment)
                                        .SelectMany(_ => _).Select(_=>_.File.Path.FullPath).ToList()).ToList();


            vsAppDomain.SetData(RESHARPER_PLUGIN_ASSEMBLIES, plugins);
            _assembliesChanged.Set();
        }

        void RegisterFileListener()
        {
            Action<IFile> up = file => _timer.Change(100, Timeout.Infinite);

            _changeMonitor = _rootDirectory.FileChanges("*.wrapdesc", modified: up, deleted: up, renamed: up, created: up);
        }

        string DetectResharperLoading(AppDomain vsAppDomain)
        {   
            while (_retries <= MAX_RETRIES)
            {
                System.Reflection.Assembly resharperAssembly;
                try
                {
                    resharperAssembly = vsAppDomain.Load("JetBrains.Platform.ReSharper.Shell");
                }
                catch
                {
                    resharperAssembly = null;
                }
                if (resharperAssembly == null && _retries++ <= MAX_RETRIES)
                {
                    System.Threading.Thread.Sleep(TimeSpan.FromSeconds(2));
                    continue;
                }
                var resharperVersion = resharperAssembly.GetName().Version;
                return resharperVersion.Major + "" + resharperVersion.Minor;
            }
            return null;
        }
    }
}