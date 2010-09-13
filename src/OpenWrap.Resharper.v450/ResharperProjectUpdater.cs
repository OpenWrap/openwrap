using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Application;
using JetBrains.ProjectModel;
using JetBrains.Util;
using OpenWrap.Exports;
using OpenWrap.Build;
using OpenWrap.Dependencies;
using OpenFileSystem.IO;
using OpenWrap.Repositories;
using OpenWrap.Services;

namespace OpenWrap.Resharper
{
    public class ResharperProjectUpdater : IWrapAssemblyClient
    {
        readonly IFile _descriptorPath;
        readonly IPackageRepository _packageRepository;
        readonly string _projectFilePath;
        static readonly Key ISWRAP = new Key("FromOpenWrap");

        public ResharperProjectUpdater(IFile descriptorPath, IPackageRepository packageRepository, string projectFilePath, ExecutionEnvironment environment)
        {
            _descriptorPath = descriptorPath;
            _packageRepository = packageRepository;
            _projectFilePath = projectFilePath;
            Environment = environment;
            WrapServices.GetService<IWrapDescriptorMonitoringService>()
                .ProcessWrapDescriptor(_descriptorPath, _packageRepository, this);
        }


        public void WrapAssembliesUpdated(IEnumerable<IAssemblyReferenceExportItem> assemblyPaths)
        {
            var assemblyFilePaths = assemblyPaths.Select(x => x.FullPath).ToList();
            try
            {
                Shell.Instance.Invocator.ReentrancyGuard
                    .ExecuteOrQueue("Updating OpenWrap references",
                                    () =>
                                    {
                                        //Debugger.Launch();
                                        using (WriteLockCookie.Create())
                                        {
                                            if (SolutionManager.Instance == null) return;

                                            ISolution solution = SolutionManager.Instance.CurrentSolution;
                                            if (solution == null) return;

                                            var project = SolutionManager.Instance.CurrentSolution.GetAllProjects()
                                                .FirstOrDefault(x => x.ProjectFile != null && x.ProjectFile.Location.FullPath == _projectFilePath);

                                            if (project == null) return;

                                            var assemblyReferences = project.GetAssemblyReferences()
                                                .Where(x => x.GetProperty(ISWRAP) != null).ToList();
                                            var existingAssemblies = assemblyReferences
                                                .Select(x => x.HintLocation.FullPath).ToList();

                                            foreach (var path in assemblyFilePaths.Where(x => !existingAssemblies.Contains(x)))
                                            {
                                                var assembly = project.AddAssemblyReference(path);
                                                assembly.SetProperty(ISWRAP, true);
                                            }
                                            foreach (var toRemove in existingAssemblies.Where(x => !assemblyFilePaths.Contains(x)))
                                            {
                                                project.RemoveModuleReference(assemblyReferences.First(x => x.HintLocation.FullPath == toRemove));
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