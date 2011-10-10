using System;
using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.IO;
using OpenWrap.Repositories;
using OpenWrap.Testing;

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
    public class locking_existing_locked_package_different_version : contexts.folder
    {
        public locking_existing_locked_package_different_version()
        {
            given_file(repository_directory.GetFile("packages.lock"),
                       "lock: name=sauron;version=1.0.0.0");
            given_package("sauron", "1.0.0.0");
            given_package("sauron", "1.0.0.1");
            given_folder_repository(FolderRepositoryOptions.SupportLocks);

            when_locking_package("sauron", "1.0.0.1");
        }

        [Test]
        public void lock_is_persisted()
        {
            repository_directory.GetFile("packages.lock").ReadString()
                .ShouldBe("lock: name=sauron; version=1.0.0.1\r\n");
        }
    }
    public class locking_new : contexts.folder
    {
        public locking_new()
        {

            given_package("sauron", "1.0.0.0");
            given_folder_repository(FolderRepositoryOptions.SupportLocks);

            when_locking_package("sauron", "1.0.0.0");
        }

        [Test]
        public void lock_is_persisted()
        {
            repository_directory.GetFile("packages.lock").ReadString()
                .ShouldBe("lock: name=sauron; version=1.0.0.0\r\n");
        }
    }
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