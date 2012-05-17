using System;
using System.Linq;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.Local;
using OpenWrap.IO.Packaging;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;

namespace Tests.Repositories.folder.context
{
    public class folder_based_repository : IDisposable
    {
        protected ITemporaryDirectory RepositoryPath;
        protected FolderRepository Repository;
        protected IPackageInfo Descriptor;
        protected PackageDependency Dependency;
        protected IFileSystem FileSystem;

        protected void given_folder_repository_with_module(string packageName)
        {
            FileSystem = LocalFileSystem.Instance;
            RepositoryPath = FileSystem.CreateTempDirectory();
            Packager.NewWithDescriptor(
                RepositoryPath.GetFile(packageName + "-1.0.0.wrap"), 
                packageName,
                "1.0.0",
                "depends: nhibernate-core = 2.1"
                );

            Repository = new FolderRepository(RepositoryPath);
        }

        protected void when_reading_test_module_descriptor(string packageName)
        {
            Descriptor = Repository.PackagesByName[packageName].FirstOrDefault();

            Dependency = Descriptor.Dependencies.FirstOrDefault();
        }
        public void Dispose()
        {
            RepositoryPath.Dispose();
        }
    }
}