﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.Local;
using OpenWrap.IO;
using OpenWrap.PackageManagement;
using OpenWrap.PackageManagement.Exporters;
using OpenWrap.PackageManagement.Monitoring;
using OpenWrap.PackageModel;
using OpenWrap.PackageModel.Serialization;
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

        public bool GenerateSharedAssemblyInfo { get; set; }

        [Output]
        public ITaskItem GeneratedSharedAssemblyInfo { get; set; } 

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
            TryGenerateAssemblyInfo();
            return RefreshWrapDependencies();
        }

        void TryGenerateAssemblyInfo()
        {
            if (!GenerateSharedAssemblyInfo)
                return;
            var file = BuildManagement.TryGenerateAssemblyInfo(WrapDescriptorPath);
            if (file != null)
                GeneratedSharedAssemblyInfo = new TaskItem(file.Path.FullPath);
        }

        public void AssembliesUpdated(IEnumerable<Exports.IAssembly> assemblyPaths)
        {
            

            var items = new List<ITaskItem>();
            
            var excludedAssemblies = ExcludeAssemblies != null ? ExcludeAssemblies.Select(x => x.ItemSpec).ToList() : new List<string>(0);

            var assemblies = assemblyPaths.Distinct(new PathComparer()).ToList();
            foreach (var assemblyRef in assemblies)
            {
                if (excludedAssemblies.Contains(assemblyRef.AssemblyName.Name, StringComparer.OrdinalIgnoreCase))
                {
                    Log.LogMessage("Ignoring OpenWrap reference to '{0}'", assemblyRef.File.Path.FullPath);
                    continue;
                }
                if (InputReferences.Any(x=>string.Equals(x.ItemSpec, assemblyRef.AssemblyName.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    Log.LogMessage("OpenWrap reference to '{0}' already added", assemblyRef.File.Path.FullPath);
                    continue;
                }
                Log.LogMessage("Adding OpenWrap reference to '{0}'", assemblyRef.File.Path.FullPath);
                var item = new TaskItem(assemblyRef.AssemblyName.FullName);
                item.SetMetadata("HintPath", assemblyRef.File.Path.FullPath);
                item.SetMetadata("Private", CopyLocal ? "True" : "False");
                item.SetMetadata("FromOpenWrap", "True");
                items.Add(item);
            }
            OutputReferences = items.ToArray();
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
            
            monitoringService.RegisterListener(WrapDescriptorPath, PackageRepository, this);
            return true;
        }
    }
}
