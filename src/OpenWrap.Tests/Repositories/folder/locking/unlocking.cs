using NUnit.Framework;
using OpenWrap.IO;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests.Repositories.folder.locking
{
    public class unlocking : contexts.folder
    {
        public unlocking()
        {
            given_package("sauron", "1.0.0.0");
            given_folder_repository(FolderRepositoryOptions.SupportLocks);

            when_unlocking_package("sauron");
        }

        [Test]
        public void lock_is_persisted()
        {
            repository_directory.GetFile("packages.lock").ReadString()
                .ShouldBeEmpty();
        }
    }
}