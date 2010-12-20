extern alias resharper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using OpenWrap.Build;
using OpenWrap.Dependencies;
using OpenFileSystem.IO;
using OpenWrap.Exports;
using OpenWrap.Repositories;
using OpenWrap.Services;
using JetBrainsKey = resharper::JetBrains.Util.Key;


namespace OpenWrap.Resharper
{
    internal class ResharperProjectUpdater : IPackageAssembliesListener
    {
        static readonly JetBrainsKey ISWRAP = new JetBrainsKey("FromOpenWrap");
        readonly resharper::JetBrains.ProjectModel.IProject _project;
        readonly IEnumerable<string> _ignoredAssemblies;
        readonly Func<ExecutionEnvironment> _env;
        readonly object _lock = new object();
        public ResharperProjectUpdater(resharper::JetBrains.ProjectModel.IProject project, Func<ExecutionEnvironment> env)
        {
            _project = project;
            _ignoredAssemblies = ReadIgnoredAssemblies();
            _env = env;
        }

        IEnumerable<string> ReadIgnoredAssemblies()
        {
            return new string[0];
        }

        public void AssembliesUpdated(IEnumerable<IAssemblyReferenceExportItem> assemblies)
        {
            ResharperLocks.WriteCookie("Updating references...", () =>
            {
                string projectFilePath = _project.ProjectFile.GetPresentableProjectPath();
                
                var allAssemblyPaths = assemblies.Select(x => x.FullPath).ToList();

                var openwrapAssemblyReferences = _project.GetAssemblyReferences()
                        .Where(x => x.GetProperty(ISWRAP) != null).ToList();
                var openwrapAssemblyPaths = openwrapAssemblyReferences
                        .Select(x => x.HintLocation.FullPath).ToList();

                foreach (var path in allAssemblyPaths
                        .Where(x => openwrapAssemblyPaths.Contains(x) == false &&
                                    _ignoredAssemblies.Any(i => x.Contains(i + ".dll")) == false))
                {
                    ResharperLogger.Debug("Adding reference {0} to {1}", projectFilePath, path);

                    var assembly = _project.AddAssemblyReference(path);
                    assembly.SetProperty(ISWRAP, true);
                }
                foreach (var toRemove in openwrapAssemblyPaths.Where(x => !allAssemblyPaths.Contains(x)))
                {
                    ResharperLogger.Debug("Removing reference {0} to {1}",
                                          projectFilePath,
                                          toRemove);
                    _project.RemoveModuleReference(
                            openwrapAssemblyReferences.First(
                                    x => x.HintLocation.FullPath == toRemove));
                }
            });
        }

        public ExecutionEnvironment Environment { get { return _env(); } }

        public bool IsLongRunning
        {
            get { return true; }
        }
    }
    public static class ResharperLocks
    {
        public static void WriteCookie(string description, Action invoke)
        {
            resharper::JetBrains.Application.Shell.Instance.Invocator.ReentrancyGuard.Dispatcher
                    .Invoke(description,
                            () => resharper::JetBrains.Application.Shell.Instance.Invocator.ReentrancyGuard.ExecuteOrQueue
                                          (
                                                  description,
                                                  () => invoke()));
        }
    }

    static class ResharperLogger
    {
        public static void Debug(string text, params string[] args)
        {
            Debugger.Log(0, "resharper", DateTime.Now.ToShortTimeString() + ":" + string.Format(text, args) + "\r\n");
        }
    }
}