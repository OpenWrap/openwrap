using NUnit.Framework;
using OpenWrap.Testing;
using Tests.Repositories.contexts;

namespace Tests.Repositories.factories.indexed_folder
{
    public class from_user_file_not_index : indexed_folder_repository
    {
        public from_user_file_not_index()
        {
            given_file(@"c:\middle-earth\somewhere.txt", "test content");
            when_detecting(@"file:///c:/middle-earth/somewhere.txt");
        }

        [Test]
        public void index_created()
        {
            FileSystem.GetFile("c:\\middle-earth\\index.wraplist").Exists.ShouldBeTrue();
        }

        [Test]
        public void repository_is_built()
        {
            Repository.ShouldNotBeNull();
        }

        [Test]
        public void token_is_generated()
        {
            Repository.Token.ShouldBe("[indexed-folder]c:\\middle-earth");
        }
    }
}