extern alias resharper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using EnvDTE;
using JetBrainsKey = resharper::JetBrains.Util.Key;

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

namespace OpenWrap.Resharper
{
    public static class ProjectModelExtensions
    {
        public static void RemoveAssemblyReference(this  resharper::JetBrains.ProjectModel.IProject project, ResharperAssemblyReference assembly)
        {
            
#if v600
            var projectImpl = project as resharper::JetBrains.ProjectModel.Impl.ProjectImpl;
            if (projectImpl == null) return;
            projectImpl.DoRemoveReference(assembly);
#else
            project.RemoveModuleReference(assembly);
#endif
        }
#if v600
        public static ResharperAssemblyReference AddAssemblyReference(this resharper::JetBrains.ProjectModel.IProject project, string path)
        {
            var projectImpl = project as resharper::JetBrains.ProjectModel.Impl.ProjectImpl;
            if (projectImpl == null) return null;
            var reference = resharper::JetBrains.ProjectModel.Impl.ProjectToAssemblyReference.CreateFromLocation(project, new resharper::JetBrains.Util.FileSystemPath(path));
            projectImpl.DoAddReference(reference);
            return reference;
        }
#endif
        public static string HintLocation(this ResharperAssemblyReference assemblyRef)
        {
#if v600
            return assemblyRef.ReferenceTarget.HintLocation.FullPath;
#else
            return assemblyRef.HintLocation.FullPath;
#endif
        }
    }
#if v600
    [ResharperSolutionComponent]
#endif
    public class ProjectReferenceUpdater : IDisposable
    {
        const string ASSEMBLY_NOTIFY = "ASSEMBLY_CHANGE_NOTIFY";
        const string ASSEMBLY_DATA = "RESHARPER_ASSEMBLY_DATA";


        static readonly JetBrainsKey ISWRAP = new JetBrainsKey("FromOpenWrap");
        readonly resharper::JetBrains.ProjectModel.ISolution _solution;
        readonly resharper::JetBrains.Application.ChangeManager _changeManager;
        readonly ResharperThreading _threading;
        System.Threading.Thread _thread;
        ManualResetEvent _shutdownSync = new ManualResetEvent(false);
        Dictionary<string, List<string>> _assemblyMap;
        bool _shuttingDown = false;
        OpenWrapOutput _output;

        public ProjectReferenceUpdater(resharper::JetBrains.ProjectModel.ISolution solution, resharper::JetBrains.Application.ChangeManager changeManager, ResharperThreading threading)
        {
            _solution = solution;
            _changeManager = changeManager;
            _threading = threading;
            _output = new OpenWrapOutput();

            _output.Write("Solution opened " + solution.Name);
            _thread = new System.Threading.Thread(LoadAssemblies) { Name = "OpenWrap assembly change listener" };


            _thread.Start();
            _changeManager.Changed += HandleChanges;

        }

        void LoadAssemblies()
        {
            while (!_shuttingDown)
            {
                EventWaitHandle wait = null;
                try
                {
                    wait = EventWaitHandle.OpenExisting(System.Diagnostics.Process.GetCurrentProcess().Id + ASSEMBLY_NOTIFY);
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
                _assemblyMap = (Dictionary<string, List<string>>)AppDomain.CurrentDomain.GetData(ASSEMBLY_DATA);
                _threading.Run("Updating references.", RefreshProjects);
            }
        }

        void RefreshProjects()
        {
            foreach (var project in _solution.GetAllProjects())
            {
                if (project.ProjectFile == null) continue;
                var projectPath = project.ProjectFile.Location.FullPath;
                if (!_assemblyMap.ContainsKey(projectPath))
                    continue;
                var existingOpenWrapReferences = project.GetAssemblyReferences()
                        .Where(x => x.GetProperty(ISWRAP) != null).ToList();

                var existingOpenWrapReferencePaths = existingOpenWrapReferences
                        .Select(assemblyRef => assemblyRef.HintLocation()).ToList();

                var assemblies = _assemblyMap[projectPath];
                foreach (var path in assemblies
                        .Where(x => !existingOpenWrapReferencePaths.Contains(x)))
                {
                    ResharperLogger.Debug("Adding reference {0} to {1}", projectPath, path);

                    var assembly = project.AddAssemblyReference(path);
                    assembly.SetProperty(ISWRAP, true);
                }
                foreach (var toRemove in existingOpenWrapReferencePaths.Where(x => !assemblies.Contains(x)))
                {
                    string remove = toRemove;
                    ResharperLogger.Debug("Removing reference {0} from {1}",
                                          projectPath,
                                          toRemove);
                    project.RemoveAssemblyReference(existingOpenWrapReferences.First(x => x.HintLocation() == remove));
                }
            }
        }
        public void Dispose()
        {
            _shuttingDown = true;
            _shutdownSync.Set();
            _thread.Join();
            _changeManager.Changed -= HandleChanges;
        }

        void HandleChanges(object sender, resharper::JetBrains.Application.ChangeEventArgs changeeventargs)
        {
            var solutionChanges = changeeventargs.ChangeMap.GetChange(_solution) as resharper::JetBrains.ProjectModel.SolutionChange;
            if (solutionChanges == null)
            {
                ResharperLogger.Debug("Unknown solution change");
                return;
            }
            if (HasSolutionChanges(solutionChanges) ||
                HasProjectChanges(solutionChanges))
            {
                ResharperLogger.Debug("Scheduled refresh of projects");
                RefreshProjects();

            }
        }

        static bool HasProjectChanges(resharper::JetBrains.ProjectModel.SolutionChange solutionChanges)
        {
            var children = solutionChanges.GetChildren();
            foreach (var child in children.OfType<resharper::JetBrains.ProjectModel.ProjectItemChange>())
            {
                if (child.IsAdded || child.IsRemoved || child.IsExternallyChanged || (child.IsSubtreeChanged && HasProjectItemChanges(child)))
                    return true;
            }
            return false;
        }

        static bool HasProjectItemChanges(resharper::JetBrains.ProjectModel.ProjectItemChange child)
        {
            var children = child.GetChildren();
            return children.OfType<resharper::JetBrains.ProjectModel.AssemblyChange>().Any();
        }

        static bool HasSolutionChanges(resharper::JetBrains.ProjectModel.SolutionChange solutionChanges)
        {
            return solutionChanges.IsAdded ||
                   solutionChanges.IsRemoved ||
                   solutionChanges.IsOpeningSolution ||
                   solutionChanges.IsClosingSolution;
        }
    }
}