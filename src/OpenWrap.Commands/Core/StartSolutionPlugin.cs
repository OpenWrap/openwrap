using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using OpenWrap.PackageManagement;
using OpenWrap.PackageManagement.Monitoring;
using OpenWrap.Runtime;
using OpenWrap.Services;

namespace OpenWrap.Commands.Core
{
    [Command(Visible = false, Noun = "solutionplugin", Verb = "start")]
    public class StartSolutionPlugin : AbstractCommand, IResolvedAssembliesUpdateListener, IDisposable
    {
        public const string SOLUTION_PLUGINS_STARTED = "All solution plugins started.";
        public const string SOLUTION_PLUGIN_STARTING = "Solution plugins starting...";
        public const string SOLUTION_PLUGIN_UNLOADING = "Solution plugins stopping...";
        public const string SOLUTION_PLUGIN_UNLOADED = "All solution plugins stopped.";
        readonly IEnvironment _environment;
        ManualResetEvent _refreshRequired;
        readonly IPackageManager _manager;
        readonly IPackageDescriptorMonitor _monitor;
        readonly List<KeyValuePair<string,IDisposable>> _plugins = new List<KeyValuePair<string, IDisposable>>();
        bool _disposed;

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

        public ExecutionEnvironment Environment
        {
            get { return _environment.ExecutionEnvironment; }
        }

        public bool IsLongRunning
        {
            get { return true; }
        }

        public void AssembliesUpdated(IEnumerable<Exports.IAssembly> assemblyPaths)
        {
            if (_refreshRequired != null)
                _refreshRequired.Set();
        }

        protected override IEnumerable<ICommandOutput> ExecuteCore()
        {
            yield return new Info(SOLUTION_PLUGIN_STARTING);

            _monitor.RegisterListener(_environment.DescriptorFile, _environment.ProjectRepository, this);


            var solutionPlugins = _manager.GetProjectExports<Exports.ISolutionPlugin>(_environment.Descriptor, _environment.ProjectRepository, _environment.ExecutionEnvironment)
                .SelectMany(x => x);

            foreach (var m in solutionPlugins.SelectMany(LoadPlugin))
                yield return m;

            yield return new Info(SOLUTION_PLUGINS_STARTED);

            _refreshRequired = new ManualResetEvent(false);
            var unloadEvent = AppDomain.CurrentDomain.GetData("openwrap.vs.events.unloading") as ManualResetEvent;

            do
            {
                var events = unloadEvent == null ? new WaitHandle[] { _refreshRequired } : new WaitHandle[] { unloadEvent, _refreshRequired };
                var exitPoint = WaitHandle.WaitAny(events);
                
                if (_disposed || (events.Length > 1 && exitPoint == 0)) break;
                var newPlugins = _manager.GetProjectExports<Exports.ISolutionPlugin>(_environment.Descriptor, _environment.ProjectRepository, _environment.ExecutionEnvironment)
                    .SelectMany(x => x).ToList();
                if (solutionPlugins.Any(x => newPlugins.Contains(x) == false)) break;
                _refreshRequired.Reset();
                foreach (var m in newPlugins.Where(x => solutionPlugins.Contains(x) == false).SelectMany(LoadPlugin))
                    yield return m;
            } while (true);
            yield return new Info(SOLUTION_PLUGIN_UNLOADING);
            
            foreach (var m in _plugins.SelectMany(UnloadPlugin)) yield return m;

            _plugins.Clear();
            yield return new Info(SOLUTION_PLUGIN_UNLOADED);
            CommitSuicide();
        }
        
        IEnumerable<ICommandOutput> UnloadPlugin(KeyValuePair<string,IDisposable> plugin)
        {
                yield return new Info("Stopping plugin {0}.", plugin.Key);
                Exception error = null;
                if (!TryDo(() => plugin.Value.Dispose(), ex => error = ex))
                    yield return new Warning("An error occured while unloading plugin.\r\n" + error);
            
        }
        IEnumerable<ICommandOutput> LoadPlugin(Exports.ISolutionPlugin plugin)
        {
            yield return new Info("Starting plugin {0}.", plugin.Name);

            Exception error = null;
            if (!TryDo(()=> _plugins.Add(new KeyValuePair<string,IDisposable>(plugin.Name,plugin.Start())), ex => error = ex))
                yield return new Warning("Plugin initialization failed.\r\n" + error);
        }

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;
            _refreshRequired.Set();
        }

        static void CommitSuicide()
        {
            if (!AppDomain.CurrentDomain.IsFinalizingForUnload())
                AppDomain.Unload(AppDomain.CurrentDomain);
        }

        static bool TryDo(Action action, Action<Exception> onError)
        {
            bool success = false;
            Exception error = null;
            try
            {
                action();
                success = true;
            }
            catch (Exception e)
            {
                error = e;
            }
            if (!success)
                onError(error);
            return success;
        }
    }
}