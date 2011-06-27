using NUnit.Framework;
using OpenWrap.Testing;
using Tests.Repositories.contexts;

namespace Tests.Repositories.factories.indexed_folder
{
    public class from_file_uri_directory_exists_and_has_files : indexed_folder_repository
    {
        public from_file_uri_directory_exists_and_has_files()
        {
            given_file(@"c:\middle-earth\something.txt", "test content");
            when_detecting(@"file:///c:/middle-earth/");
        }

        [Test]
        public void index_not_created()
        {
            FileSystem.GetFile("c:\\middle-earth\\index.wraplist").Exists.ShouldBeFalse();
        }

        [Test]
        public void repository_not_built()
        {
            Repository.ShouldBeNull();
        }
    }
}