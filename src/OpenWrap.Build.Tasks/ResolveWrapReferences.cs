using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using OpenRasta.Wrap.Build;
using OpenRasta.Wrap.Build.Services;
using OpenRasta.Wrap.Dependencies;
using OpenRasta.Wrap.Repositories;
using OpenRasta.Wrap.Sources;
using OpenWrap.Resharper;

namespace OpenWrap.Build.Tasks
{
    public class ResolveWrapReferences : Task, IWrapAssemblyClient
    {
        readonly List<IWrapAssemblyClient> _resolveHooks = new List<IWrapAssemblyClient>();
        static IWrapAssemblyClient _resharperIntegration;
        FileSystemWatcher _fsMonitor;

        public ResolveWrapReferences()
        {
            InternalServices.Initialize();
        }

        [Required]
        public string Platform { get; set; }

        [Required]
        public string Profile { get; set; }

        [Required]
        public string ProjectFilePath { get; set; }

        public bool EnableVisualStudioInegration { get; set; }

        public bool CopyLocal { get; set;}

        [Output]
        public ITaskItem[] References { get; set; }

        [Required]
        public ITaskItem WrapDescriptor { get; set; }

        [Required]
        public string WrapsDirectory { get; set; }

        protected IWrapRepository WrapRepository { get; set; }

        string WrapDescriptorPath
        {
            get { return Path.GetFullPath(WrapDescriptor.ItemSpec); }
        }

        string WrapsDirectoryPath
        {
            get { return Path.GetFullPath(WrapsDirectory); }
        }

        public override bool Execute()
        {
            try
            {
                EnsureWrapRepositoryIsInitialized();

                EnableVisualStudioIntegration();
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

        void EnableVisualStudioIntegration()
        {
            if (!EnableVisualStudioInegration) return;

            WrapServices.TryRegisterService(()=> new ResharperIntegrationService(Environment));
            WrapServices.GetService<ResharperIntegrationService>()
                .TryAddNotifier(WrapDescriptorPath, WrapRepository, ProjectFilePath);
        }

        bool DependencyNotFound(WrapDependency dependency)
        {
            Log.LogError("The dependency on wrap '{0}' was not found.", dependency);
            return false;
        }

        void EnsureWrapRepositoryIsInitialized()
        {
            if (WrapRepository != null) return;
            WrapRepository = new FolderRepository(WrapsDirectoryPath);
        }

        bool RefreshWrapDependencies()
        {
            var monitoringService = WrapServices.GetService<IWrapDescriptorMonitoringService>();
            monitoringService.ProcessWrapDescriptor(WrapDescriptorPath, WrapRepository, this);
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

        public WrapRuntimeEnvironment Environment
        {
            get
            {
                return new WrapRuntimeEnvironment
                {
                    Platform = Platform,
                    Profile = Profile
                };
            }
        }
    }
}