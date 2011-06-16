using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using OpenWrap.PackageManagement;
using OpenWrap.PackageManagement.Exporters.Assemblies;
using OpenWrap.PackageManagement.Monitoring;
using OpenWrap.Runtime;
using OpenWrap.Services;

namespace OpenWrap.Commands.Core
{
    [Command(Visible = false, Noun = "solutionplugin", Verb = "start")]
    public class StartSolutionPlugin : AbstractCommand, IResolvedAssembliesUpdateListener
    {
        public const string SOLUTION_PLUGIN_STARTED = "Solution plugin started";
        IPackageManager _manager;
        IEnvironment _environment;
        IPackageDescriptorMonitor _monitor;
        IList<IDisposable> _plugins = new List<IDisposable>();
        ManualResetEvent _exit = new ManualResetEvent(false);

        public StartSolutionPlugin()
            : this(ServiceLocator.GetService<IPackageDescriptorMonitor>(), ServiceLocator.GetService<IPackageManager>(), ServiceLocator.GetService<IEnvironment>())
        {
        }

        public StartSolutionPlugin(IPackageDescriptorMonitor monitor, IPackageManager manager, IEnvironment environment)
        {
            _monitor = monitor;
            _environment = environment;
            _manager = manager;
        }

        protected override IEnumerable<ICommandOutput> ExecuteCore()
        {
            yield return new Info(SOLUTION_PLUGIN_STARTED);

            _monitor.RegisterListener(_environment.DescriptorFile, _environment.ProjectRepository, this);


            var solutionPlugins = _manager.GetProjectExports<Exports.ISolutionPlugin>(_environment.Descriptor, _environment.ProjectRepository, _environment.ExecutionEnvironment)
                .SelectMany(x => x);

            foreach(var plugin in solutionPlugins)
            {
                yield return new Info("Starting plugin {0}.", plugin.Name);
                bool loadSuccess = false;
                try
                {
                    _plugins.Add(plugin.Start());
                    loadSuccess = true;
                }
                catch
                {
                }
                if (!loadSuccess)
                {
                    yield return new Warning("Plugin initialization failed.");
                }
            }
            var solPackages = solutionPlugins
                .Select(x => x.Start())
                .ToList();

            _exit.WaitOne();
            solPackages.ForEach(x => x.Dispose());

            CommitSuicide();
        }

        static void CommitSuicide()
        {
            AppDomain.Unload(AppDomain.CurrentDomain);
        }

        public void AssembliesUpdated(IEnumerable<Exports.IAssembly> assemblyPaths)
        {
            _exit.Set();
        }

        public ExecutionEnvironment Environment
        {
            get { return _environment.ExecutionEnvironment; }
        }

        public bool IsLongRunning
        {
            get { return true; }
        }
    }
}