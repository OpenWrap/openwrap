using System;
using System.Collections.Generic;
using System.Linq;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.InMemory;
using OpenRasta.Client;
using OpenWrap;
using OpenWrap.IO.Packaging;
using OpenWrap.PackageManagement;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.Repositories.contexts
{
    public class folder_repository : OpenWrap.Testing.context
    {
        FolderRepositoryOptions _options;
        protected InMemoryFileSystem file_system;
        protected ITemporaryDirectory repository_directory;
        protected FolderRepository repository;
        protected InMemoryRepository source_repository;
        protected List<PackageCleanResult> clean_result;

        public folder_repository()
        {
            file_system = new InMemoryFileSystem();
            repository_directory = file_system.CreateTempDirectory();
            source_repository = new InMemoryRepository("somewhere");
        }
        protected void given_folder_repository(FolderRepositoryOptions options)
        {
            repository = new FolderRepository(repository_directory, options);
        }
        protected void given_package_file(string name, string version)
        {
            Packager.NewWithDescriptor(repository_directory.GetFile(PackageNameUtility.PackageFileName(name, version)), name, version);
        }
        protected void when_publishing_package(string name, string version, params string[] descriptorLines)
        {
            AddPackage(name, version, descriptorLines);
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

        protected void given_package(string name, string version, params string[] descriptorLines)
        {
            AddPackage(name, version, descriptorLines);
        }

    }
}