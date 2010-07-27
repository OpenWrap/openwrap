using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystem.InMemory;
using OpenFileSystem.IO.FileSystem.Local;
using OpenWrap.Testing;

namespace OpenWrap.Tests.IO
{
    public abstract class file_system<T> : context where T : IFileSystem
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
                .Path.FullPath.ShouldBe(@"c:\mordor\shire\galladrin\");
        }

        [Test]
        public void can_get_file_with_directory_path()
        {
            FileSystem.GetDirectory(@"c:\mordor").GetFile(@"shire\frodo.txt")
                .Path.FullPath.ShouldBe(@"c:\mordor\shire\frodo.txt");
        }
        [Test]
        public void directory_is_resolved_relative_to_current_directory()
        {
            var dir = FileSystem.GetDirectory("shire");

            dir.Path.FullPath.ShouldBe(Path.Combine(CurrentDirectory,@"shire\"));
            dir.Exists.ShouldBeFalse();
        }
        [Test]
        public void files_are_resolved_relative_to_current_directory()
        {
            FileSystem.GetFile("rohan.html").Path.FullPath
                .ShouldBe(Path.Combine(CurrentDirectory, "rohan.html"));
        }
    
        [Test]
        public void two_directories_are_equal()
        {
            FileSystem.GetDirectory("shire").ShouldBe(FileSystem.GetDirectory("shire"));
        }
        [Test]
        public void two_files_are_equal()
        {
            FileSystem.GetDirectory("rohan.html").ShouldBe(FileSystem.GetDirectory("rohan.html"));
        }
        [Test]
        public void non_existant_file_opened_for_write_is_created_automatically()
        {
            var file = FileSystem.GetFile(@"c:\mordor\elves.txt");
            file.Exists.ShouldBeFalse();
            file.OpenWrite().Close();
            file.Exists.ShouldBeTrue();
            file.Delete();
        }

        [Test]
        public void trailing_slash_is_not_significant()
        {
            var first = FileSystem.GetDirectory(@"c:\mordor");
            var second = FileSystem.GetDirectory(@"c:\mordor\");

            first.ShouldBe(second);
        }

        protected IFileSystem FileSystem { get; set; }
        protected string CurrentDirectory { get; set; }
    }

    public class in_memory_fs : file_system<InMemoryFileSystem>
    {
        public in_memory_fs()
        {
            CurrentDirectory = @"c:\mordor";
            FileSystem = new InMemoryFileSystem(
                new InMemoryDirectory(@"c:\mordor",
                    new InMemoryFile("rings.txt")
                )
            )
            {
                CurrentDirectory = CurrentDirectory
            };
        }
        [Test]
        public void can_add_folders_to_fs()
        {
            var fs = new InMemoryFileSystem(new InMemoryDirectory(@"c:\mordor"));
            fs.Directories.ShouldHaveCountOf(1);
        }
        [Test]
        public void can_add_sub_folders()
        {
            var fs = new InMemoryFileSystem(new InMemoryDirectory(@"c:\mordor\nurn"));
            var mordor = fs.GetDirectory(@"c:\mordor");
            mordor.Exists.ShouldBeTrue();

            var nurn = mordor.GetDirectory("nurn");
            nurn.Path.FullPath.ShouldBe(@"c:\mordor\nurn\");
            nurn.Exists.ShouldBeTrue();
        }
    }

    public class local_fs : file_system<LocalFileSystem>
    {
        public local_fs(){
            CurrentDirectory = Environment.CurrentDirectory;

            FileSystem = LocalFileSystem.Instance;
        }
    }
    public class path_specification : context
    {
        [Test]
        public void path_has_segments()
        {
            var path = new LocalPath(@"c:\mordor\nurn");
            path.Segments.ShouldHaveSameElementsAs(new[] { @"c:\", "mordor", "nurn" });
        }
        [Test]
        public void trailing_slash_is_always_normalized()
        {
            new LocalPath(@"c:\mordor\nurn").ShouldBe(new LocalPath(@"c:\mordor\nurn\"));
        }
    }
}
