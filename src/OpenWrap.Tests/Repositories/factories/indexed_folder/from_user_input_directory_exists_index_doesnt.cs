using NUnit.Framework;
using OpenWrap.Testing;
using Tests.Repositories.contexts;

namespace Tests.Repositories.factories.indexed_folder
{
    [TestFixture("file:///c:/middle-earth/index.wraplist")]
    [TestFixture("file:///c:/middle-earth/")]
    [TestFixture("file:///c:/middle-earth")]
    public class from_user_input_directory_exists_index_doesnt : indexed_folder_repository
    {
        public from_user_input_directory_exists_index_doesnt(string userInput)
        {
            given_directory("c:\\middle-earth\\");

            when_detecting(userInput);
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
    [TestFixture("http://server.com")]
    [TestFixture("c:\\folder")]
    [TestFixture(@"\\server\share\folder")]
    class from_unrecognized_user_input : indexed_folder_repository
    {
        public from_unrecognized_user_input(string userInput)
        {
            when_detecting(userInput);
        }

        [Test]
        public void repository_is_not_built()
        {
            Repository.ShouldBeNull();
        }
    }
}