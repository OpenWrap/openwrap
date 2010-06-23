using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using OpenWrap.Exports;
using OpenWrap.Build;
using OpenWrap.Dependencies;
using OpenWrap.IO;
using OpenWrap.Repositories;
using OpenWrap.Resharper;
using OpenWrap.Services;

namespace OpenWrap.Build.Tasks
{
    public class ResolveWrapReferences : Task, IWrapAssemblyClient
    {
        readonly List<IWrapAssemblyClient> _resolveHooks = new List<IWrapAssemblyClient>();
        static IWrapAssemblyClient _resharperIntegration;
        FileSystemWatcher _fsMonitor;
        IFileSystem _fileSystem;

        public ResolveWrapReferences()
        {
            InternalServices.Initialize();
            _fileSystem = FileSystems.Local;
        }

        [Required]
        public string Platform { get; set; }

        [Required]
        public string Profile { get; set; }

        [Required]
        public string ProjectFilePath { get; set; }

        public bool EnableVisualStudioIntegration { get; set; }

        public bool CopyLocal { get; set;}

        [Output]
        public ITaskItem[] References { get; set; }

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

                HookupToVisualStudio();
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

        void HookupToVisualStudio()
        {
            if (!EnableVisualStudioIntegration) return;
            try
            {
                EnableResharperIntegration();
            }
            catch(Exception e){}
            WrapServices.GetService<ResharperIntegrationService>()
                .TryAddNotifier(WrapDescriptorPath, PackageRepository, ProjectFilePath);
        }

        void EnableResharperIntegration()
        {
            WrapServices.TryRegisterService(()=> new ResharperIntegrationService(Environment));
        }

        bool DependencyNotFound(WrapDependency dependency)
        {
            Log.LogError("The dependency on wrap '{0}' was not found.", dependency);
            return false;
        }

        void EnsureWrapRepositoryIsInitialized()
        {
            if (PackageRepository != null) return;
            PackageRepository = new FolderRepository(WrapsDirectoryPath);
        }

        bool RefreshWrapDependencies()
        {
            var monitoringService = WrapServices.GetService<IWrapDescriptorMonitoringService>();
            monitoringService.ProcessWrapDescriptor(WrapDescriptorPath, PackageRepository, this);
            return true;
        }

        public bool IsLongRunning { get { return false; } }
        public void WrapAssembliesUpdated(IEnumerable<IAssemblyReferenceExportItem> assemblyPaths)
        {
            var items = new List<ITaskItem>();
            foreach (var assemblyRef in assemblyPaths)
            {
                var item = new TaskItem(assemblyRef.AssemblyName.FullName);
                item.SetMetadata("HintPath", assemblyRef.FullPath);
                item.SetMetadata("Private", CopyLocal ? "True" : "False");
                item.SetMetadata("FromOpenWrap", "True");
                items.Add(item);
            }
            References = items.ToArray();
        }

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
    }
}