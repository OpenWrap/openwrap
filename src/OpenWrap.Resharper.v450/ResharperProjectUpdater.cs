extern alias resharper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenFileSystem.IO;
using OpenWrap.PackageManagement.Exporters;
using OpenWrap.PackageManagement.Monitoring;
using OpenWrap.Runtime;
using OpenWrap.Services;
using JetBrainsKey = resharper::JetBrains.Util.Key;


namespace OpenWrap.Resharper
{
    internal class ResharperProjectUpdater : IResolvedAssembliesUpdateListener
    {
        static readonly JetBrainsKey ISWRAP = new JetBrainsKey("FromOpenWrap");
        readonly Func<ExecutionEnvironment> _env;
        readonly IEnumerable<string> _ignoredAssemblies;
        readonly object _lock = new object();
        readonly resharper::JetBrains.ProjectModel.IProject _project;

        public ResharperProjectUpdater(resharper::JetBrains.ProjectModel.IProject project, Func<ExecutionEnvironment> env)
        {
            _project = project;
            _ignoredAssemblies = ReadIgnoredAssemblies();
            _env = env;
        }
        public string ProjectPath { get { return _project.ProjectFile.Location.FullPath; } }
        public ExecutionEnvironment Environment
        {
            get { return _env(); }
        }

        public bool IsLongRunning
        {
            get { return true; }
        }

        public IFile Descriptor
        {
            get
            {
                // TODO: Detect which file is currently being monitored by the project
                return ServiceLocator.GetService<IEnvironment>().DescriptorFile;
            }
        }

        public void AssembliesUpdated(IEnumerable<IAssemblyReferenceExportItem> assemblies)
        {
            ResharperLocks.WriteCookie("Updating references...",
                                       () =>
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

        static IEnumerable<string> ReadIgnoredAssemblies()
        {
            // TODO: https://github.com/openrasta/openwrap/issues/issue/125
            return new string[0];
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

    internal static class ResharperLogger
    {
        public static void Debug(string text, params string[] args)
        {
            Debugger.Log(0, "resharper", DateTime.Now.ToShortTimeString() + ":" + string.Format(text, args) + "\r\n");
        }
    }
}