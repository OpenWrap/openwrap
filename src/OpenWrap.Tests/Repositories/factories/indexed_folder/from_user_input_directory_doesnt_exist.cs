using NUnit.Framework;
using OpenWrap.Testing;
using Tests.Repositories.contexts;

namespace Tests.Repositories.factories.indexed_folder
{
    [TestFixture("file:///c:/middle-earth")]
    [TestFixture("file:///c:/middle-earth/")]
    [TestFixture("file:///c:/middle-earth/index.wraplist")]
    public class from_user_input_directory_doesnt_exist : indexed_folder_repository
    {
        public from_user_input_directory_doesnt_exist(string userInput)
        {
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
            Repository.Token.ShouldBe("[indexed-folder]c:\\middle-earth\\index.wraplist");
        }
    }
}