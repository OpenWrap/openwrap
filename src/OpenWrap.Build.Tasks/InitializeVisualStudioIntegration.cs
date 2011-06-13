using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using OpenWrap.Build.Tasks.Hooks;
using OpenWrap.Collections;
using OpenWrap.Commands;
using OpenWrap.PackageManagement;
using OpenWrap.PackageManagement.Exporters;
using OpenWrap.Repositories;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.Local;
using OpenWrap.Runtime;
using OpenWrap.Services;
using OpenWrap.VisualStudio.Hooks;


namespace OpenWrap.Build.Tasks
{
    public class InitializeVisualStudioIntegration : Task
    {
        public bool EnableVisualStudioIntegration { get; set; }

        [Required]
        public string Platform { get; set; }

        [Required]
        public string Profile { get; set; }

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

        IFile WrapDescriptorPath
        {
            get { return LocalFileSystem.Instance.GetFile(WrapDescriptor.ItemSpec); }
        }

        protected IPackageRepository PackageRepository { get; set; }
        [Required]
        public ITaskItem WrapDescriptor { get; set; }

        [Required]
        public string WrapsDirectory { get; set; }

        public ITaskItem[] ExcludeAssemblies { get; set; }

        IDirectory WrapsDirectoryPath
        {
            get { return LocalFileSystem.Instance.GetDirectory(WrapsDirectory); }
        }
        void EnsureWrapRepositoryIsInitialized()
        {
            if (PackageRepository != null)
            {
                Log.LogMessage(MessageImportance.Low, "Project repository found.");
                return;
            }
            PackageRepository = new FolderRepository(WrapsDirectoryPath);
        }
        [Required]
        public string ProjectFilePath { get; set; }

        public InitializeVisualStudioIntegration()
        {
        }
        public override bool Execute()
        {
            ResharperLogger.Debug("Initialize called on " + ProjectFilePath);
            //Debugger.Launch();
            EnsureWrapRepositoryIsInitialized();
            
            if (!EnableVisualStudioIntegration) return true;
            SolutionAddIn.Initialize();
            ResharperHook.TryRegisterResharper(Environment, WrapDescriptorPath, PackageRepository);
            
            return true;
        }
    }
}