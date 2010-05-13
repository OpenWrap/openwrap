using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Application;
using JetBrains.ProjectModel;
using JetBrains.Util;
using OpenRasta.Wrap.Build;
using OpenRasta.Wrap.Build.Services;
using OpenRasta.Wrap.Dependencies;
using OpenRasta.Wrap.Repositories;
using OpenRasta.Wrap.Sources;

namespace OpenWrap.Resharper
{
    public class ResharperProjectUpdater : IWrapAssemblyClient
    {
        readonly string _descriptorPath;
        readonly IWrapRepository _wrapRepository;
        readonly string _projectFilePath;
        static readonly Key ISWRAP = new Key("FromOpenWrap");

        public ResharperProjectUpdater(string descriptorPath, IWrapRepository wrapRepository, string projectFilePath, WrapRuntimeEnvironment environment)
        {
            _descriptorPath = descriptorPath;
            _wrapRepository = wrapRepository;
            _projectFilePath = projectFilePath;
            Environment = environment;
            WrapServices.GetService<IWrapDescriptorMonitoringService>()
                .ProcessWrapDescriptor(_descriptorPath, _wrapRepository, this);
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

        public WrapRuntimeEnvironment Environment
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