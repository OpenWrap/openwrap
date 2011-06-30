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