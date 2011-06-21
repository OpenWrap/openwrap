using System;
using System.Linq;
using System.Threading;
using EnvDTE;
using OpenWrap.IO;
using OpenWrap.PackageManagement;
using OpenWrap.PackageManagement.Exporters.Assemblies;
using OpenWrap.PackageManagement.Monitoring;
using OpenWrap.Runtime;
using OpenWrap.Services;
using OpenWrap.VisualStudio.ProjectModel;

namespace OpenWrap.VisualStudio.SolutionPlugins.ReSharper
{
    public class DteProjectModelPlugin : IDisposable
    {
        readonly IEnvironment _environment;
        readonly IPackageDescriptorMonitor _monitor;
        const string ASSEMBLY_NOTIFY = "ASSEMBLY_CHANGE_NOTIFY";
        const string ASSEMBLY_DATA = "RESHARPER_ASSEMBLY_DATA";
        bool _running;
        DTE _dte;
        DteSolution _solution;
        IPackageManager _packageManager;
        EventWaitHandle _assembliesChanged;

        public DteProjectModelPlugin()
        {
            try
            {
                _dte = SiteManager.GetGlobalService<DTE>();
            }
            catch
            {
                _running = false;
                return;
            }
            _solution = new DteSolution(_dte.Solution);
            _solution.ProjectChanged += HandleProjectChange;
            _environment = ServiceLocator.GetService<IEnvironment>();
            _packageManager = ServiceLocator.GetService<IPackageManager>();
            var vsAppDomain = AppDomain.CurrentDomain.GetData("openwrap.vs.appdomain") as AppDomain;
            _assembliesChanged = new EventWaitHandle(false, EventResetMode.AutoReset, System.Diagnostics.Process.GetCurrentProcess().Id + ASSEMBLY_NOTIFY);
            
            RefreshProjects();
        }

        void HandleProjectChange(object sender, EventArgs e)
        {
            RefreshProjects();
        }

        void RefreshProjects()
        {
            var projects =
                _solution.AllProjects.Where(x => x.OpenWrapEnabled)
                                     .Select(x => new
                                     {
                                         project = x,
                                         scope = PathFinder.GetCurrentScope(_environment.Descriptor.DirectoryStructure, x.File.Path)
                                     });
            var resolvedAssemblies = (
                                         from project in projects
                                         where _environment.ScopedDescriptors.ContainsKey(project.scope)
                                         let descriptor = _environment.ScopedDescriptors[project.scope].Value
                                         let ee = new ExecutionEnvironment(project.project.TargetPlatform, project.project.TargetFramework.ToOpenWrapMoniker())
                                         let projectAssemblyReferences = _packageManager.GetProjectAssemblyReferences(descriptor, _environment.ProjectRepository, ee, false)

                                         select new { path = project.project.File.Path.FullPath, asm = projectAssemblyReferences.Select(x => x.File.Path.FullPath).ToList() }
                                     ).ToDictionary(_ => _.path, _ => _.asm);

            var vsAppDomain = AppDomain.CurrentDomain.GetData("openwrap.vs.appdomain") as AppDomain;

            vsAppDomain.SetData(ASSEMBLY_DATA, resolvedAssemblies);
            _assembliesChanged.Set();
        }

        public void Dispose()
        {
            if (!_running) return;
            _solution.ProjectChanged -= HandleProjectChange;
            _solution.Dispose();
            _dte = null;
        }
    }
}