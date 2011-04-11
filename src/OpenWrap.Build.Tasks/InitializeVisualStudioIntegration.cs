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
        static UICommands _commands;
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
            InternalServices.Initialize();
            
        }
        public override bool Execute()
        {
            ResharperLogger.Debug("Initialize called on " + ProjectFilePath);
            EnsureWrapRepositoryIsInitialized();
            
            if (!EnableVisualStudioIntegration) return true;
            ResharperHook.TryRegisterResharper(Environment, WrapDescriptorPath, PackageRepository);
            SolutionAddIn.Initialize();
            if (_commands == null)
            {
                lock (this)
                    if (_commands == null)
                    {
                        var repository = new CommandRepository(ReadCommands(Services.ServiceLocator.GetService<IEnvironment>()));
                        _commands = new UICommands(repository);
                        _commands.Initialize();
                    }
            }
            return true;
        }

        static IEnumerable<ICommandDescriptor> ReadCommands(IEnvironment environment)
        {
            throw new NotImplementedException();
            //return Services.ServiceLocator.GetService<IPackageExporter>()
            //        .GetExports<IExport>("commands", environment.ExecutionEnvironment, new[] { environment.ProjectRepository, environment.SystemRepository }.NotNull())
            //        .SelectMany(x => x.Items)
            //        .OfType<ICommandExportItem>()
            //        .Select(x => x.Descriptor).ToList();
        }

    }

}