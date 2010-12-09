using System.Diagnostics;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using OpenWrap.Dependencies;
using OpenWrap.Repositories;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.Local;

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
                Log.LogMessage(MessageImportance.Low, "No project repository found.");
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

            ResharperHook.TryRegisterResharper(Environment, WrapDescriptorPath, PackageRepository, ProjectFilePath, ExcludeAssemblies.Select(x=>x.ItemSpec).ToList());
            return true;
        }
    }
}