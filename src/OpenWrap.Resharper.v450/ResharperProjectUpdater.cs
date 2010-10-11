extern alias resharper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenWrap.Exports;
using OpenWrap.Build;
using OpenWrap.Dependencies;
using OpenFileSystem.IO;
using OpenWrap.Repositories;
using OpenWrap.Services;
using Shell = resharper::JetBrains.Application.Shell;
using JetBrainsKey = resharper::JetBrains.Util.Key;
using WriteLockCookie = resharper::JetBrains.Application.WriteLockCookie;
using SolutionManager = resharper::JetBrains.ProjectModel.SolutionManager;
using ISolution = resharper::JetBrains.ProjectModel.ISolution;

namespace OpenWrap.Resharper
{
    public class ResharperProjectUpdater : IWrapAssemblyClient
    {
        readonly IFile _descriptorPath;
        
        readonly IPackageRepository _packageRepository;
        readonly string _projectFilePath;
        static readonly JetBrainsKey ISWRAP = new JetBrainsKey("FromOpenWrap");
        readonly List<string> _ignoredAssemblies;

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


        public void WrapAssembliesUpdated(IEnumerable<IAssemblyReferenceExportItem> assemblyPaths)
        {
            var allAssemblyPaths = assemblyPaths.Select(x => x.FullPath).ToList();
            try
            {
                Shell.Instance.Invocator.ReentrancyGuard
                    .ExecuteOrQueue("Updating OpenWrap references",
                                    () =>
                                    {
                                        using (WriteLockCookie.Create())
                                        {
                                            if (SolutionManager.Instance == null) return;

                                            ISolution solution = SolutionManager.Instance.CurrentSolution;
                                            if (solution == null) return;

                                            var project = System.Linq.Enumerable.FirstOrDefault(SolutionManager.Instance.CurrentSolution.GetAllProjects(), x => x.ProjectFile != null && x.ProjectFile.Location.FullPath == _projectFilePath);

                                            if (project == null) return;
                                            
                                            var openwrapAssemblyReferences = project.GetAssemblyReferences()
                                                .Where(x => x.GetProperty(ISWRAP) != null).ToList();
                                            var openwrapAssemblyPaths = openwrapAssemblyReferences
                                                .Select(x => x.HintLocation.FullPath).ToList();

                                            foreach (var path in allAssemblyPaths
                                                .Where(x => openwrapAssemblyPaths.Contains(x) == false &&
                                                            _ignoredAssemblies.Any(i=>x.Contains(i + ".dll")) == false))
                                            {
                                                var assembly = project.AddAssemblyReference(path);
                                                assembly.SetProperty(ISWRAP, true);
                                            }
                                            foreach (var toRemove in openwrapAssemblyPaths.Where(x => !allAssemblyPaths.Contains(x)))
                                            {
                                                project.RemoveModuleReference(openwrapAssemblyReferences.First(x => x.HintLocation.FullPath == toRemove));
                                            }
                                        }

                                    });
            }
            catch(Exception e)
            {
                Debug.WriteLine("Exception when updating resharper: \r\n" + e);
            }
        }

        public ExecutionEnvironment Environment
        {
            get;
            set;
        }

        public bool IsLongRunning
        {
            get{ return true; }
        }

    }
}