using System;
using System.Collections.Generic;
using System.Linq;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.InMemory;
using OpenWrap;
using OpenWrap.IO;
using OpenWrap.IO.Packaging;
using OpenWrap.PackageManagement;
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
        protected List<PackageCleanResult> clean_result;
        protected InMemoryRepository source_repository;

        public folder()
        {
            file_system = new InMemoryFileSystem();
            repository_directory = file_system.CreateTempDirectory();
            source_repository = new InMemoryRepository("somewhere");
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

        protected void given_package(string name, string version, params string[] descriptorLines)
        {
            if (repository == null)
                given_folder_repository();
            AddPackage(name, version, descriptorLines);
        }

        protected void when_locking_package(string name, string version)
        {
            repository.Lock(string.Empty, repository.PackagesByName[name].Where(x => x.Version == version.ToVersion()));
        }
        protected void when_unlocking_package(string name)
        {
            repository.Unlock(string.Empty, repository.PackagesByName[name].Where(x=>x.Name.EqualsNoCase(name)));
        }

        protected void when_publishing_package(string name, string version, params string[] descriptorLines)
        {
            AddPackage(name, version, descriptorLines);
        }
        protected void when_cleaning_package(string name, string version)
        {
            repository.RefreshPackages();
            clean_result = repository.Clean(repository.PackagesByName[name].Where(_ => _.Version != version.ToVersion())
                .Concat(repository.PackagesByName.Where(_ => _.Key.EqualsNoCase(name) == false).SelectMany(_ => _))).ToList();
        }

        void AddPackage(string name, string version, string[] descriptorLines)
        {
            var packageFile = Packager.NewWithDescriptor(file_system.CreateTempFile(), name, version, descriptorLines);

            using (var publisher = (IPackagePublisherWithSource)repository.Publisher())
            using (var packageFileStream = packageFile.OpenRead())
            {
                publisher.Publish(source_repository, name + "-" + version + ".wrap", packageFileStream);
            }
        }
    }
}