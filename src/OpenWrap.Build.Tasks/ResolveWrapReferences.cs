using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.Local;
using OpenWrap.PackageManagement;
using OpenWrap.PackageManagement.Exporters;
using OpenWrap.PackageManagement.Monitoring;
using OpenWrap.Repositories;
using OpenWrap.Runtime;
using OpenWrap.Services;

namespace OpenWrap.Build.Tasks
{

    public class ResolveWrapReferences : Task, IResolvedAssembliesUpdateListener
    {
        readonly IFileSystem _fileSystem;

        public ResolveWrapReferences()
        {
            _fileSystem = LocalFileSystem.Instance;
        }

        
        public bool CopyLocal { get; set; }

        public ITaskItem[] InputReferences { get; set; }
        public ExecutionEnvironment Environment
        {
            get
            {
                return new ExecutionEnvironment
                {
                        Platform = Platform,
                        Profile = Profile
                };
            }
        }

        public bool IsLongRunning
        {
            get { return false; }
        }

        public void AssembliesError(string errorMessage)
        {
            Log.LogError(errorMessage);
        }

        [Required]
        public string Platform { get; set; }

        [Required]
        public string Profile { get; set; }

        public ITaskItem[] ExcludeAssemblies { get; set; }

        [Output]
        public ITaskItem[] OutputReferences { get; set; }

        [Output]
        public ITaskItem[] ReferenceCopyLocalPaths { get; set; }

        [Required]
        public ITaskItem WrapDescriptor { get; set; }

        [Required]
        public string WrapsDirectory { get; set; }

        protected IPackageRepository PackageRepository { get; set; }

        IFile WrapDescriptorPath
        {
            get { return _fileSystem.GetFile(WrapDescriptor.ItemSpec); }
        }

        IDirectory WrapsDirectoryPath
        {
            get { return _fileSystem.GetDirectory(WrapsDirectory); }
        }

        public override bool Execute()
        {
            //Debugger.Launch();

            try
            {
                EnsurePackageRepositoryIsInitialized();
            }
            catch (FileNotFoundException e)
            {
                Log.LogErrorFromException(e, false, false, WrapDescriptor.ItemSpec);
                return false;
            }
            catch (DirectoryNotFoundException e)
            {
                Log.LogErrorFromException(e, false, false, WrapDescriptor.ItemSpec);
                return false;
            }
            return RefreshWrapDependencies();
        }
        
        public void AssembliesUpdated(IEnumerable<Exports.IAssembly> assemblyPaths)
        {
            var references = new List<ITaskItem>();
            var referenceCopyLocalPaths = new List<ITaskItem>();
            
            var excludedAssemblies = ExcludeAssemblies != null ? ExcludeAssemblies.Select(x => x.ItemSpec).ToList() : new List<string>(0);

            var assemblies = assemblyPaths.Distinct(new PathComparer()).ToList();
            foreach (var assemblyRef in assemblies)
            {
                var fullPath = assemblyRef.File.Path.FullPath;
                if (excludedAssemblies.Contains(assemblyRef.AssemblyName.Name, StringComparer.OrdinalIgnoreCase))
                {
                    Log.LogMessage("Ignoring OpenWrap reference to '{0}'", fullPath);
                    continue;
                }
                if (InputReferences.Any(x=>string.Equals(x.ItemSpec, assemblyRef.AssemblyName.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    Log.LogMessage("OpenWrap reference to '{0}' already added", fullPath);
                    continue;
                }


                if (assemblyRef.IsAssemblyReference)
                {
                    Log.LogMessage("Adding OpenWrap reference to '{0}'", fullPath);
                    var item = new TaskItem(assemblyRef.AssemblyName.FullName);
                    item.SetMetadata("HintPath", fullPath);
                    item.SetMetadata("Private", CopyLocal ? "True" : "False");
                    item.SetMetadata("FromOpenWrap", "True");
                    references.Add(item);
                }
                else if (assemblyRef.IsRuntimeAssembly)
                {
                    if (!CopyLocal)
                    {
                        Log.LogMessage("Not adding '{0}' to ReferenceCopyLocalPaths because CopyLocal is false.", fullPath);
                        continue;
                    }

                    Log.LogMessage("Adding OpenWrap runtime-only reference to '{0}'", fullPath);
                    var item = new TaskItem(fullPath);
                    item.SetMetadata("FromOpenWrap", "True");
                    referenceCopyLocalPaths.Add(item);
                }
            }
            OutputReferences = references.ToArray();
            ReferenceCopyLocalPaths = referenceCopyLocalPaths.ToArray();
        }

        class PathComparer : IEqualityComparer<Exports.IAssembly>
        {

            public bool Equals(Exports.IAssembly x, Exports.IAssembly y)
            {
                return x != null && y != null && x.AssemblyName.Name == y.AssemblyName.Name;
            }

            public int GetHashCode(Exports.IAssembly obj)
            {

                return ReferenceEquals(obj, null) || obj.AssemblyName == null || obj.AssemblyName.Name == null
                    ? 0
                    : obj.AssemblyName.Name.GetHashCode();
            }
        }

        void EnsurePackageRepositoryIsInitialized()
        {
            if (PackageRepository != null)
            {
                Log.LogMessage(MessageImportance.Low, "No project repository found.");
                return;
            }
            PackageRepository = new FolderRepository(WrapsDirectoryPath, FolderRepositoryOptions.SupportLocks);
            PackageRepository.RefreshPackages();
        }


        bool RefreshWrapDependencies()
        {
            var monitoringService = Services.ServiceLocator.GetService<IPackageDescriptorMonitor>();
            
            // Note: registering a listener implicitly notifies the newly registered listener
            monitoringService.RegisterListener(WrapDescriptorPath, PackageRepository, this);
            return true;
        }
    }
}
