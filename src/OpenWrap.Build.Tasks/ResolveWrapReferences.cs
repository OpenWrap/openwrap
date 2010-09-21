using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using OpenFileSystem.IO;
using OpenWrap.Dependencies;
using OpenWrap.Exports;
using OpenWrap.Repositories;
using OpenWrap.Services;
using OpenFileSystem.IO.FileSystem.Local;

namespace OpenWrap.Build.Tasks
{

    public class ResolveWrapReferences : Task, IWrapAssemblyClient
    {
        readonly IFileSystem _fileSystem;

        public ResolveWrapReferences()
        {
            InternalServices.Initialize();
            _fileSystem = LocalFileSystem.Instance;
        }

        
        public bool CopyLocal { get; set; }

        public bool EnableVisualStudioIntegration { get; set; }

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

        [Required]
        public string Platform { get; set; }

        [Required]
        public string Profile { get; set; }

        [Required]
        public string ProjectFilePath { get; set; }

        
        public ITaskItem[] ExcludeAssemblies { get; set; }

        [Output]
        public ITaskItem[] OutputReferences { get; set; }

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
            try
            {
                EnsureWrapRepositoryIsInitialized();
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
        
        public void WrapAssembliesUpdated(IEnumerable<IAssemblyReferenceExportItem> assemblyPaths)
        {
            

            var items = new List<ITaskItem>();
            
            var excludedAssemblies = ExcludeAssemblies != null ? ExcludeAssemblies.Select(x => x.ItemSpec).ToList() : new List<string>(0);

            var assemblies = assemblyPaths.Distinct(new PathComparer()).ToList();
            foreach (var assemblyRef in assemblies)
            {
                if (excludedAssemblies.Contains(assemblyRef.AssemblyName.Name, StringComparer.OrdinalIgnoreCase))
                {
                    Log.LogMessage("Ignoring OpenWrap reference to '{0}'", assemblyRef.FullPath);
                    continue;
                }
                if (InputReferences.Any(x=>string.Equals(x.ItemSpec, assemblyRef.AssemblyName.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    Log.LogMessage("OpenWrap reference to '{0}' already added", assemblyRef.FullPath);
                    continue;
                }
                Log.LogMessage("Adding OpenWrap reference to '{0}'", assemblyRef.FullPath);
                var item = new TaskItem(assemblyRef.AssemblyName.FullName);
                item.SetMetadata("HintPath", assemblyRef.FullPath);
                item.SetMetadata("Private", CopyLocal ? "True" : "False");
                item.SetMetadata("FromOpenWrap", "True");
                items.Add(item);
            }
            OutputReferences = items.ToArray();
        }

        class PathComparer : IEqualityComparer<IAssemblyReferenceExportItem>
        {

            public bool Equals(IAssemblyReferenceExportItem x, IAssemblyReferenceExportItem y)
            {
                return x != null && y != null && x.AssemblyName.Name == y.AssemblyName.Name;
            }

            public int GetHashCode(IAssemblyReferenceExportItem obj)
            {

                return ReferenceEquals(obj, null) || obj.AssemblyName == null || obj.AssemblyName.Name == null
                    ? 0
                    : obj.AssemblyName.Name.GetHashCode();
            }
        }

        void EnsureWrapRepositoryIsInitialized()
        {
            if (PackageRepository != null)
            {
                Log.LogMessage(MessageImportance.Low, "No project repository found.");
                return;
            }
            PackageRepository = new FolderRepository(WrapsDirectoryPath, false);
            PackageRepository.Refresh();
        }


        bool RefreshWrapDependencies()
        {
            var monitoringService = WrapServices.GetService<IWrapDescriptorMonitoringService>();
            
            monitoringService.ProcessWrapDescriptor(WrapDescriptorPath, PackageRepository, this);
            return true;
        }
    }
}