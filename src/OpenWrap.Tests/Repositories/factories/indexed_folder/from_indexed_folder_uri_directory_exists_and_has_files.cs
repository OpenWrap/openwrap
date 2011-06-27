using NUnit.Framework;
using OpenWrap.Testing;
using Tests.Repositories.contexts;

namespace Tests.Repositories.factories.indexed_folder
{
    public class from_indexed_folder_uri_directory_exists_and_has_files : indexed_folder_repository
    {
        public from_indexed_folder_uri_directory_exists_and_has_files()
        {
            given_file(@"c:\middle-earth\something.txt", "test content");
            when_detecting(@"indexed-folder:///c:/middle-earth/");
        }

        [Test]
        public void index_created()
        {
            FileSystem.GetFile("c:\\middle-earth\\index.wraplist").Exists.ShouldBeTrue();
        }

        [Test]
        public void repository_not_built()
        {
            Repository.ShouldNotBeNull();
        }
        [Test]
        public void token_is_generated()
        {
            Repository.Token.ShouldBe("[indexed-folder]c:\\middle-earth\\index.wraplist");
        }
    }
}