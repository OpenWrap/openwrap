using System;
using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Repositories;

namespace Tests.Repositories.folder.locking
{
    public class existing_lock : contexts.folder
    {
        public existing_lock()
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