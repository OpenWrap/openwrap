using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenWrap.IO;
using OpenWrap.Testing;

namespace OpenWrap.Tests.IO
{
    public class file_system<T> : context where T : IFileSystem
    {
        [Test]
        public void directories_always_created()
        {
            var directory = FileSystem.GetDirectory(@"c:\test\test.html");

            directory.Exists.ShouldBeFalse();
        }
        [Test]
        public void created_directory_exists()
        {
            var directory = FileSystem.CreateDirectory(@"c:\test\temp.html");

            directory.Exists.ShouldBeTrue();
        }
        [Test]
        public void can_get_subdirectory_of_non_existant_directory()
        {
            FileSystem.GetDirectory(@"c:\mordor").GetDirectory(@"shire\galladrin")
                .Path.FullPath.ShouldBe(@"c:\mordor\shire\galladrin");
        }
        [Test]
        public void directory_is_resolved_relative_to_current_directory()
        {
            var dir = FileSystem.GetDirectory("shire");

            dir.Path.FullPath.ShouldBe(Path.Combine(CurrentDirectory,@"shire"));
            dir.Exists.ShouldBeFalse();
        }
        [Test]
        public void two_directories_are_equal()
        {
            FileSystem.GetDirectory("shire").ShouldBe(FileSystem.GetDirectory("shire"));
        }
        protected T FileSystem { get; set; }
        protected string CurrentDirectory { get; set; }
    }

    public class in_memory_fs : file_system<InMemoryFileSystem>
    {
        public in_memory_fs()
        {
            CurrentDirectory = @"c:\mordor";
            FileSystem = new InMemoryFileSystem(
                //new InMemoryDirectory(@"c:\mordor",
                //    new InMemoryFile("rings.txt")
                //    )
                )
            {
                CurrentDirectory = CurrentDirectory
            };
        }

    }

    public class local_fs : file_system<LocalFileSystem>
    {
        public local_fs(){
            CurrentDirectory = Environment.CurrentDirectory;

            FileSystem = new LocalFileSystem();
        }
    }
}
