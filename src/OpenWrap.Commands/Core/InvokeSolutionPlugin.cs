using System;
using System.Collections.Generic;
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
        IPackageManager _manager;
        IEnvironment _environment;
        IPackageDescriptorMonitor _monitor;
        ManualResetEvent _exit = new ManualResetEvent(false);

        public StartSolutionPlugin() : this(ServiceLocator.GetService<IPackageDescriptorMonitor>())
        {
        }

        public StartSolutionPlugin(IPackageDescriptorMonitor monitor)
        {
            _monitor = monitor;
        }

        protected override IEnumerable<ICommandOutput> ExecuteCore()
        {
            yield return new Info("Solution plugin started");
            
            //_monitor.RegisterListener(_environment.DescriptorFile, _environment.ProjectRepository, this);


            //var solPackages = _manager.GetProjectExports<Exports.ISolutionPlugin>(_environment.Descriptor, _environment.ProjectRepository, _environment.ExecutionEnvironment)
            //    .SelectMany(x => x)
            //    .Select(x => x.Start())
            //    .ToList();

            //_exit.WaitOne();
            //solPackages.ForEach(x => x.Dispose());

            //CommitSuicide();
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