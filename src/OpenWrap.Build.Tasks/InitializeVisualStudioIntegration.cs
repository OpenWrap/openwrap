using System.Diagnostics;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using OpenWrap.Dependencies;
using OpenWrap.Repositories;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystem.Local;

namespace OpenWrap.Build.Tasks
{
    public class InitializeVisualStudioIntegration : Task
    {
        public bool EnableVisualStudioIntegration { get; set; }
        static object _resharperHook;

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
            PackageRepository = new FolderRepository(WrapsDirectoryPath, false);
        }
        [Required]
        public string ProjectFilePath { get; set; }

        public InitializeVisualStudioIntegration()
        {
            InternalServices.Initialize();
            
        }
        public override bool Execute()
        {
            EnsureWrapRepositoryIsInitialized();
            if (!EnableVisualStudioIntegration) return true;
            if (_resharperHook != null)
                return true;
            _resharperHook = ResharperHook.TryRegisterResharper(Environment, WrapDescriptorPath, PackageRepository, ProjectFilePath);
            return true;
        }
    }
}