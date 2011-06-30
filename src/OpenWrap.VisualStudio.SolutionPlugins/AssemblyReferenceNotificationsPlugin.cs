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

namespace OpenWrap.SolutionPlugins.VisualStudio
{
    public class AssemblyReferenceNotificationsPlugin : IDisposable
    {
        const string ASSEMBLY_DATA = "RESHARPER_ASSEMBLY_DATA";
        const string ASSEMBLY_NOTIFY = "ASSEMBLY_CHANGE_NOTIFY";
        readonly EventWaitHandle _assembliesChanged;
        readonly IPackageManager _packageManager;
        readonly IPackageRepository _projectRepository;
        readonly IDirectory _rootDirectory;
        readonly DteSolution _solution;
        readonly Timer _timer;
        IDisposable _changeMonitor;
        bool _running;

        public AssemblyReferenceNotificationsPlugin()
        {
            DTE dte;
            try
            {
                dte = SiteManager.GetGlobalService<DTE>();
            }
            catch
            {
                _running = false;
                return;
            }
            _solution = new DteSolution(dte.Solution);
            _solution.ProjectChanged += HandleProjectChange;
            var environment = ServiceLocator.GetService<IEnvironment>();
            _packageManager = ServiceLocator.GetService<IPackageManager>();

            _assembliesChanged = new EventWaitHandle(false, EventResetMode.AutoReset, System.Diagnostics.Process.GetCurrentProcess().Id + ASSEMBLY_NOTIFY);
            _rootDirectory = environment.DescriptorFile.Parent;
            _projectRepository = environment.ProjectRepository;
            RegisterFileListener();
            RefreshProjects();
            _timer = new Timer(_ => RefreshProjects(), null, Timeout.Infinite, Timeout.Infinite);
        }

        public void Dispose()
        {
            if (!_running) return;
            _assembliesChanged.Close();
            _changeMonitor.Dispose();
            _running = false;
            _solution.ProjectChanged -= HandleProjectChange;
            _solution.Dispose();
            
        }

        void RefreshProjects()
        {
            var descriptors = new PackageDescriptorReader().ReadAll(_rootDirectory);
            var projects =
                _solution.AllProjects.Where(x => x.OpenWrapEnabled)
                    .Select(x => new
                    {
                        project = x,
                        scope = PathFinder.GetCurrentScope(descriptors[string.Empty].Value.DirectoryStructure, x.File.Path)
                    });
            var resolvedAssemblies = (
                                         from project in projects
                                         where descriptors.ContainsKey(project.scope)
                                         let descriptor = descriptors[project.scope].Value
                                         let ee = new ExecutionEnvironment(project.project.TargetPlatform, project.project.TargetFramework.ToOpenWrapMoniker())
                                         let projectAssemblyReferences = _packageManager.GetProjectAssemblyReferences(descriptor, _projectRepository, ee, false)
                                         select new { path = project.project.File.Path.FullPath, asm = projectAssemblyReferences.Select(x => x.File.Path.FullPath).ToList() }
                                     ).ToDictionary(_ => _.path, _ => _.asm);

            var vsAppDomain = (AppDomain)AppDomain.CurrentDomain.GetData("openwrap.vs.appdomain");

            vsAppDomain.SetData(ASSEMBLY_DATA, resolvedAssemblies);
            _assembliesChanged.Set();
        }

        void RegisterFileListener()
        {
            Action<IFile> up = file => _timer.Change(100, Timeout.Infinite);

            _changeMonitor = _rootDirectory.FileChanges("*.wrapdesc", modified: up, deleted: up, renamed: up, created: up);
        }

        void HandleProjectChange(object sender, EventArgs e)
        {
            RefreshProjects();
        }
    }
}