using System;
using System.Linq;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.InMemory;
using OpenWrap.IO;
using OpenWrap.IO.Packaging;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests.Repositories.contexts
{
    public abstract class folder : context
    {
        InMemoryFileSystem file_system;
        protected ITemporaryDirectory repository_directory;
        protected FolderRepository repository;
        ILookup<string, IPackageInfo> locked_packages;

        public folder()
        {
            file_system = new InMemoryFileSystem();
            repository_directory = file_system.CreateTempDirectory();
        }
        protected void given_folder_repository(FolderRepositoryOptions options = FolderRepositoryOptions.Default)
        {
            repository = new FolderRepository(repository_directory, options);
        }

        protected void when_reading_locked_packages()
        {
            locked_packages = repository.LockedPackages;
        }

        protected void given_file(IFile file, string content)
        {
            file.WriteString(content);
        }

        protected void given_package(string name, string version)
        {
            Packager.NewWithDescriptor(repository_directory.GetFile(PackageNameUtility.PackageFileName(name, version)),
                                       name,
                                       version);
        }
    }
}