extern alias resharper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using EnvDTE;
using OpenWrap.IO;
using OpenWrap.Runtime;
using JetBrainsKey = resharper::JetBrains.Util.Key;

namespace OpenWrap.Resharper
{
    [resharper::JetBrains.ProjectModel.SolutionComponentImplementationAttribute]
    public class ProjectReferenceUpdater : resharper::JetBrains.ProjectModel.ISolutionComponent
    {
        const string ASSEMBLY_NOTIFY = "ASSEMBLY_CHANGE_NOTIFY";
        const string ASSEMBLY_DATA = "RESHARPER_ASSEMBLY_DATA";


        static readonly JetBrainsKey ISWRAP = new JetBrainsKey("FromOpenWrap");
        readonly resharper::JetBrains.ProjectModel.ISolution _solution;
        System.Threading.Thread _thread;
        ManualResetEvent _shutdownSync = new ManualResetEvent(false);
        Dictionary<string, List<string>> _assemblyMap;
        bool _shuttingDown = false;
        OpenWrapOutput _output;

        public ProjectReferenceUpdater(resharper::JetBrains.ProjectModel.ISolution solution)
        {
            _solution = solution;
            _output = new OpenWrapOutput();

            _output.Write("Solution opened " + solution.Name);
            _thread = new System.Threading.Thread(LoadAssemblies) { Name = "OpenWrap assembly change listener" };



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
                Guard.Run("Updating references.", RefreshProjects);
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
                        .Select(x => x.HintLocation.FullPath).ToList();

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
                    project.RemoveModuleReference(
                            existingOpenWrapReferences.First(
                                    x => x.HintLocation.FullPath == remove));
                }
            }
        }

        public void Init()
        {
            _thread.Start();
            resharper::JetBrains.Application.ChangeManager.Instance.Changed += HandleChanges;
        }

        public void Dispose()
        {
            resharper::JetBrains.Application.ChangeManager.Instance.Changed -= HandleChanges;
        }

        public void AfterSolutionOpened()
        {
            _thread.Start();
        }

        public void BeforeSolutionClosed()
        {
            _shuttingDown = true;
            _shutdownSync.Set();
            _thread.Join();
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

        bool HasProjectChanges(resharper::JetBrains.ProjectModel.SolutionChange solutionChanges)
        {
            var children = solutionChanges.GetChildren();
            foreach (var child in children.OfType<resharper::JetBrains.ProjectModel.ProjectItemChange>())
            {
                if (child.IsAdded || child.IsRemoved || child.IsExternallyChanged || (child.IsSubtreeChanged && HasProjectItemChanges(child)))
                    return true;
            }
            return false;
        }

        bool HasProjectItemChanges(resharper::JetBrains.ProjectModel.ProjectItemChange child)
        {
            var children = child.GetChildren();
            return children.OfType<resharper::JetBrains.ProjectModel.AssemblyChange>().Any();
        }

        bool HasSolutionChanges(resharper::JetBrains.ProjectModel.SolutionChange solutionChanges)
        {
            return solutionChanges.IsAdded ||
                   solutionChanges.IsRemoved ||
                   solutionChanges.IsOpeningSolution ||
                   solutionChanges.IsClosingSolution;
        }
    }

    public static class Guard
    {
        public static void Run(Action invoke)
        {
            Run(invoke.Method.Name, invoke);

        }
        public static void Run(string description, Action invoke)
        {
            resharper::JetBrains.Application.Shell.Instance.Invocator.ReentrancyGuard.Dispatcher
                    .Invoke(description,
                            () => resharper::JetBrains.Application.Shell.Instance.Invocator.ReentrancyGuard.Execute
                                          (description, () => invoke()));
        }
        public static bool BeginInvokeAndWait<T>(string description, Func<T> invoker, out T value, params WaitHandle[] waitHandles)
        {
            var shell = resharper::JetBrains.Application.Shell.Instance;
            var guard = shell.Invocator.ReentrancyGuard;
            var disp = guard.Dispatcher;
            T returnValue = default(T);
            ManualResetEvent finished = new ManualResetEvent(false);
            disp.BeginOrInvoke(description, () => guard.Execute(description, () => { returnValue = invoker();
                                                                                       finished.Set();}));
            var handles = new WaitHandle[waitHandles.Length + 1];
            waitHandles[0] = finished;
            waitHandles.CopyTo(handles, 1);

            var breakage = WaitHandle.WaitAny(waitHandles);
            value = returnValue;
            return breakage == 0;
        }
    }

    internal static class ResharperLogger
    {
        public static void Debug(string text, params string[] args)
        {
            System.Diagnostics.Debugger.Log(0, "resharper", DateTime.Now.ToShortTimeString() + ":" + string.Format(text, args) + "\r\n");
        }
    }
}