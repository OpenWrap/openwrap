extern alias resharper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using OpenFileSystem.IO;
using OpenWrap.PackageManagement;
using OpenWrap.PackageManagement.Monitoring;
using OpenWrap.Runtime;
using JetBrainsKey = resharper::JetBrains.Util.Key;


namespace OpenWrap.Resharper
{
    internal class ResharperProjectUpdater : IResolvedAssembliesUpdateListener
    {
        static readonly JetBrainsKey ISWRAP = new JetBrainsKey("FromOpenWrap");
        readonly Func<ExecutionEnvironment> _env;
        readonly IEnumerable<string> _ignoredAssemblies;
        readonly resharper::JetBrains.ProjectModel.IProject _project;

        public ResharperProjectUpdater(IFile descriptor, resharper::JetBrains.ProjectModel.IProject project, Func<ExecutionEnvironment> env)
        {
            _project = project;
            _ignoredAssemblies = ReadIgnoredAssemblies();
            Descriptor = descriptor;
            _env = env;
        }

        public IFile Descriptor { get; private set; }

        public ExecutionEnvironment Environment
        {
            get { return _env(); }
        }

        public bool IsLongRunning
        {
            get { return true; }
        }

        public string ProjectPath
        {
            get { return _project.ProjectFile == null ? string.Empty : _project.ProjectFile.Location.FullPath; }
        }

        public void AssembliesUpdated(IEnumerable<Exports.IAssembly> resolvedAssemblies)
        {

            Guard.Run("Updating references...",
                                       () =>
                                       {
                                           if (_project.ProjectFile == null) return;
                                           string projectFilePath = _project.ProjectFile.GetPresentableProjectPath();

                                           var resolvedAssemblyPaths = resolvedAssemblies.Select(x => x.File.Path.FullPath).ToList();

                                           var owProjectAssemblyReferences = _project.GetAssemblyReferences()
                                                   .Where(x => x.GetProperty(ISWRAP) != null).ToList();

                                           var owProjectAssemblyReferencePaths = owProjectAssemblyReferences
                                                   .Select(x => x.HintLocation.FullPath).ToList();

                                           foreach (var path in resolvedAssemblyPaths
                                                   .Where(x => !owProjectAssemblyReferencePaths.Contains(x) &&
                                                               _ignoredAssemblies.Any(i => x.Contains(i + ".dll")) == false))
                                           {
                                               ResharperLogger.Debug("Adding reference {0} to {1}", projectFilePath, path);

                                               var assembly = _project.AddAssemblyReference(path);
                                               assembly.SetProperty(ISWRAP, true);
                                           }
                                           foreach (var toRemove in owProjectAssemblyReferencePaths.Where(x => !resolvedAssemblyPaths.Contains(x)))
                                           {
                                               string remove = toRemove;
                                               ResharperLogger.Debug("Removing reference {0} to {1}",
                                                                     projectFilePath,
                                                                     toRemove);
                                               _project.RemoveModuleReference(
                                                       owProjectAssemblyReferences.First(
                                                               x => x.HintLocation.FullPath == remove));
                                           }
                                       });
        }

        static IEnumerable<string> ReadIgnoredAssemblies()
        {
            // TODO: https://github.com/openrasta/openwrap/issues/issue/125
            return new string[0];
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
    }

    internal static class ResharperLogger
    {
        public static void Debug(string text, params string[] args)
        {
            Debugger.Log(0, "resharper", DateTime.Now.ToShortTimeString() + ":" + string.Format(text, args) + "\r\n");
        }
    }
}