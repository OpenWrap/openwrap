using System;
using System.Collections.Generic;
using System.Linq;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.Local;
using OpenWrap.Configuration;
using OpenWrap.PackageModel;
using OpenWrap.PackageModel.Serialization;
using OpenWrap.Repositories;

namespace OpenWrap.Runtime
{
    public class CurrentDirectoryEnvironment : IEnvironment
    {
        public CurrentDirectoryEnvironment()
            : this(LocalFileSystem.Instance.GetDirectory(System.Environment.CurrentDirectory))
        {
        }

        public CurrentDirectoryEnvironment(IDirectory currentDirectory)
        {
            CurrentDirectory = currentDirectory;
            BeforeProjectRepositoryInitialized = (dir, options) => { };
        }

        public IDirectory ConfigurationDirectory { get; private set; }
        public IDirectory CurrentDirectory { get; set; }

        public IPackageRepository CurrentDirectoryRepository { get; set; }

        IDictionary<string, FileBased<IPackageDescriptor>> _scopedDescriptors = new Dictionary<string, FileBased<IPackageDescriptor>>();
        public IDictionary<string, FileBased<IPackageDescriptor>> ScopedDescriptors
        {
            get { TryInitializeProject(); return _scopedDescriptors; }
        }

        public IPackageDescriptor Descriptor
        {
            get { TryInitializeProject(); return _scopedDescriptors.ContainsKey(string.Empty) ? _scopedDescriptors[string.Empty].Value : null; }
        }

        public IFile DescriptorFile
        {
            get { TryInitializeProject(); return _scopedDescriptors.ContainsKey(string.Empty) ? _scopedDescriptors[string.Empty].File : null; }
        }

        public ExecutionEnvironment ExecutionEnvironment { get; private set; }
        public IFileSystem FileSystem { get; set; }
        IPackageRepository _projectRepository;
        public IPackageRepository ProjectRepository
        {
            get
            {
                if (_projectRepository == null)
                    TryInitializeProject();

                return _projectRepository;
            }
            set { _projectRepository = value; }
        }
        
        public IPackageRepository SystemRepository { get; private set; }
        public IDirectory SystemRepositoryDirectory { get; set; }

        public void Initialize()
        {

            FileSystem = LocalFileSystem.Instance;
            SystemRepositoryDirectory = SystemRepositoryDirectory ?? FileSystem.GetDirectory(DefaultInstallationPaths.SystemRepositoryDirectory);

            TryInitializeProject();

            CurrentDirectoryRepository = new CurrentDirectoryRepository();

            SystemRepository = new FolderRepository(SystemRepositoryDirectory)
            {
                Name = "System repository"
            };

            ConfigurationDirectory = FileSystem.GetDirectory(DefaultInstallationPaths.ConfigurationDirectory);


            ExecutionEnvironment = new ExecutionEnvironment
            {
                Platform = IntPtr.Size == 4 ? "x86" : "x64",
                Profile = Environment.Version.Major >= 4 ? "net40" : "net35"
            };
        }

        void TryInitializeProject()
        {
            if (_scopedDescriptors.Count > 0) return;

            _scopedDescriptors = new PackageDescriptorReader().ReadAll(CurrentDirectory);
            if (_scopedDescriptors.Count == 0) return;
            if (_scopedDescriptors.ContainsKey(string.Empty))
                TryInitializeProjectRepository();
        }

        public Action<IDirectory, FolderRepositoryOptions> BeforeProjectRepositoryInitialized { get; set; }
        void TryInitializeProjectRepository()
        {
            if (Descriptor.UseProjectRepository)
            {
                var projectRepositoryDirectory = DescriptorFile.Parent.FindProjectRepositoryDirectory().MustExist();


                var repositoryOptions = FolderRepositoryOptions.AnchoringEnabled;
                if (Descriptor.UseSymLinks)
                    repositoryOptions |= FolderRepositoryOptions.UseSymLinks;
                if (Descriptor.StorePackages)
                    repositoryOptions |= FolderRepositoryOptions.PersistPackages;
                BeforeProjectRepositoryInitialized(projectRepositoryDirectory, repositoryOptions);
                _projectRepository = new FolderRepository(projectRepositoryDirectory, repositoryOptions)
                {
                    Name = "Project repository"
                };
            }
        }
    }
}
