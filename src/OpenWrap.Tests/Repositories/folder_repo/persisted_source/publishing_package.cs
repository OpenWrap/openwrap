using System;
using NUnit.Framework;
using OpenFileSystem.IO;
using OpenWrap.IO;
using OpenWrap.IO.Packaging;
using OpenWrap.PackageManagement.Packages;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests.Repositories.folder_repo.persisted_source
{
    public class publishing_package : contexts.folder_repository
    {
        public publishing_package()
        {
            given_folder_repository(FolderRepositoryOptions.PersistPackageSources);
            when_publishing_package("test", "1.0.0");
        }

        [Test]
        public void package_should_be_published()
        {
            repository.PackagesByName.Count.ShouldBe(1);
        }

        [Test]
        public void folder_should_have_index()
        {
            repository_directory.GetFile("packages").Exists.ShouldBeTrue();
        }

        [Test]
        public void index_should_contain_package()
        {
            repository_directory.GetFile("packages").ReadString()
                .ShouldBe("package: name=test; source=[memory]somewhere; version=1.0.0\r\n");
        }
    }
}