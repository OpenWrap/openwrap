extern alias resharper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using OpenFileSystem.IO;
using OpenWrap.Build;
using OpenWrap.Dependencies;
using OpenWrap.Exports;
using OpenWrap.Repositories;
using OpenWrap.Services;
using JetBrainsKey = resharper::JetBrains.Util.Key;

namespace OpenWrap.Resharper
{
    public class ResharperProjectUpdater : IPackageAssembliesListener
    {
        static readonly JetBrainsKey ISWRAP = new JetBrainsKey("FromOpenWrap");
        readonly IFile _descriptorPath;
        readonly List<string> _ignoredAssemblies;

        readonly IPackageRepository _packageRepository;
        readonly string _projectFilePath;
        bool _succeeded;

        Timer _timer;

        public ResharperProjectUpdater(IFile descriptorPath, IPackageRepository packageRepository, string projectFilePath, ExecutionEnvironment environment, IEnumerable<string> ignoredAssemblies)
        {
            _descriptorPath = descriptorPath;
            _packageRepository = packageRepository;
            _projectFilePath = projectFilePath;
            _ignoredAssemblies = ignoredAssemblies.ToList();
            Environment = environment;

            Services.Services.GetService<IWrapDescriptorMonitoringService>()
                    .ProcessWrapDescriptor(_descriptorPath, _packageRepository, this);
        }

        public ExecutionEnvironment Environment { get; set; }

        public bool IsLongRunning
        {
            get { return true; }
        }

        public void AssembliesUpdated(IEnumerable<IAssemblyReferenceExportItem> assemblyPaths)
        {
            var allAssemblyPaths = assemblyPaths.Select(x => x.FullPath).ToList();
            try
            {
                resharper::JetBrains.Application.Shell.Instance.Invocator.ReentrancyGuard.Dispatcher.Invoke("Updating OpenWrap references",
                    () =>
                    {
                        resharper::JetBrains.Application.Shell.Instance.Invocator.ReentrancyGuard.ExecuteOrQueue(
                                "Updating OpenWrap references",
                                () =>
                                {
                                    using (resharper::JetBrains.Application.WriteLockCookie.Create())
                                    {
                                        if (resharper::JetBrains.ProjectModel.SolutionManager.Instance == null) return;
                                        ResharperLogger.Debug("SolutionManager found");

                                        resharper::JetBrains.ProjectModel.ISolution solution = resharper::JetBrains.ProjectModel.SolutionManager.Instance.CurrentSolution;
                                        if (solution == null) return;
                                        ResharperLogger.Debug("Solution found");

                                        _succeeded = true;

                                        var project = solution.GetAllProjects().FirstOrDefault(x => x.ProjectFile != null && x.ProjectFile.Location.FullPath == _projectFilePath);

                                        if (project == null) return;

                                        var openwrapAssemblyReferences = project.GetAssemblyReferences()
                                                .Where(x => x.GetProperty(ISWRAP) != null).ToList();
                                        var openwrapAssemblyPaths = openwrapAssemblyReferences
                                                .Select(x => x.HintLocation.FullPath).ToList();

                                        foreach (var path in allAssemblyPaths
                                                .Where(x => openwrapAssemblyPaths.Contains(x) == false &&
                                                            _ignoredAssemblies.Any(i => x.Contains(i + ".dll")) == false))
                                        {
                                            ResharperLogger.Debug("Adding reference {0} to {1}", _projectFilePath, path);

                                            var assembly = project.AddAssemblyReference(path);
                                            assembly.SetProperty(ISWRAP, true);
                                        }
                                        foreach (var toRemove in openwrapAssemblyPaths.Where(x => !allAssemblyPaths.Contains(x)))
                                        {
                                            ResharperLogger.Debug("Removing reference {0} to {1}", _projectFilePath, toRemove);
                                            project.RemoveModuleReference(openwrapAssemblyReferences.First(x => x.HintLocation.FullPath == toRemove));
                                        }
                                    }
                                });
                    });
            }
            catch (Exception e)
            {
                ResharperLogger.Debug("Exception when updating resharper: \r\n" + e);
            }
            if (!_succeeded)
            {
                ResharperLogger.Debug("Import failed, rescheduling in one second.");

                _timer = new Timer(s => AssembliesUpdated((IEnumerable<IAssemblyReferenceExportItem>)s), assemblyPaths, 1000, Timeout.Infinite);
            }
        }
    }

    public class MessageBoxStuff : resharper::JetBrains.ProjectModel.ISolutionComponent
    {
        public void Init()
        {
        }

        public void Dispose()
        {
        }

        public void AfterSolutionOpened()
        {
            resharper::JetBrains.Util.MessageBox.ShowInfo("Solution opened.");
        }

        public void BeforeSolutionClosed()
        {
        }
    }
}