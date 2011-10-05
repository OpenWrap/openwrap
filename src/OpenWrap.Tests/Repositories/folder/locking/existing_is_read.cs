using NUnit.Framework;
using OpenWrap.Repositories;

namespace Tests.Repositories.folder.locking
{
    public class existing_is_read : contexts.folder
    {
        public existing_is_read()
        {
            given_file(repository_directory.GetFile("packages.lock"), 
                       "lock: name=sauron;version=1.0.0.0");
            given_package("sauron", "1.0.0.0");
            given_folder_repository(FolderRepositoryOptions.SupportLocks);
            when_reading_locked_packages();
        }

        [Test]
        public void package_is_locked()
        {
            repository.ShouldHaveLock("sauron", "1.0.0.0");
        }
    }
}