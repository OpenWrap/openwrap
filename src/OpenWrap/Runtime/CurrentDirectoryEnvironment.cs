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
        {
            CurrentDirectory = LocalFileSystem.Instance.GetDirectory(System.Environment.CurrentDirectory);
        }

        public CurrentDirectoryEnvironment(string currentDirectory)
        {
            CurrentDirectory = LocalFileSystem.Instance.GetDirectory(currentDirectory);
        }

        public IDirectory ConfigurationDirectory { get; private set; }
        public IDirectory CurrentDirectory { get; set; }

        public IPackageRepository CurrentDirectoryRepository { get; set; }

        public PackageDescriptor Descriptor { get; set; }
        public IFile DescriptorFile { get; private set; }
        public ExecutionEnvironment ExecutionEnvironment { get; private set; }
        public IFileSystem FileSystem { get; set; }
        public IPackageRepository ProjectRepository { get; set; }
        public IEnumerable<IPackageRepository> RemoteRepositories { get; set; }
        public IPackageRepository SystemRepository { get; set; }

        public void Initialize()
        {
            FileSystem = LocalFileSystem.Instance;
            DescriptorFile = CurrentDirectory
                    .AncestorsAndSelf()
                    .SelectMany(x => x.Files("*.wrapdesc"))
                    .FirstOrDefault();
            if (DescriptorFile != null)
                Descriptor = new PackageDescriptorReaderWriter().Read(DescriptorFile);

            TryInitializeProjectRepository();

            CurrentDirectoryRepository = new CurrentDirectoryRepository();

            SystemRepository = new FolderRepository(FileSystem.GetDirectory(DefaultInstallationPaths.SystemRepositoryDirectory))
            {
                    Name = "System repository"
            };

            ConfigurationDirectory = FileSystem.GetDirectory(DefaultInstallationPaths.ConfigurationDirectory);

            RemoteRepositories = new RemoteRepositoryBuilder(FileSystem, Services.Services.GetService<IConfigurationManager>()).GetConfiguredPackageRepositories().ToList();
            ExecutionEnvironment = new ExecutionEnvironment
            {
                    Platform = IntPtr.Size == 4 ? "x86" : "x64",
                    Profile = Environment.Version.Major >= 4 ? "net40" : "net35"
            };
        }

        void TryInitializeProjectRepository()
        {
            if (Descriptor == null || DescriptorFile == null)
                return;
            if (Descriptor.UseProjectRepository)
            {
                var projectRepositoryDirectory = DescriptorFile.Parent.FindProjectRepositoryDirectory();


                if (projectRepositoryDirectory != null)
                {
                    var repositoryOptions = FolderRepositoryOptions.AnchoringEnabled;
                    if (Descriptor.UseSymLinks)
                        repositoryOptions |= FolderRepositoryOptions.UseSymLinks;
                    ProjectRepository = new FolderRepository(projectRepositoryDirectory, repositoryOptions)
                    {
                            Name = "Project repository"
                    };
                }
            }
        }
    }
}