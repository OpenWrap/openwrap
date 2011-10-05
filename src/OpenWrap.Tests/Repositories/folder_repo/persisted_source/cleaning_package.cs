using NUnit.Framework;
using OpenWrap.IO;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests.Repositories.folder_repo.persisted_source
{
    public class cleaning_package : contexts.folder
    {
        public cleaning_package()
        {
            given_folder_repository(FolderRepositoryOptions.PersistPackageSources);
            given_package("test", "1.0.0");
            given_package("test", "1.0.1");
            when_cleaning_package("test", "1.0.0");
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
        public void index_should_not_contain_cleaned_package()
        {
            repository_directory.GetFile("packages").ReadString()
                .ShouldBe("package: name=test; source=[memory]somewhere; version=1.0.1\r\n");
        }
    }
}