using System;
using NUnit.Framework;
using OpenWrap.Testing;
using Tests.Repositories.contexts;

namespace Tests.Repositories.factories.indexed_folder
{
    [TestFixture("file:///c:/middle-earth")]
    [TestFixture("file:///c:/middle-earth")]
    [TestFixture("file:///c:/middle-earth")]
    [TestFixture("indexed-folder:///c:/middle-earth")]
    [TestFixture("indexed-folder:///c:/middle-earth")]
    [TestFixture("indexed-folder:///c:/middle-earth")]
    public class from_user_input_index_exists : indexed_folder_repository
    {
        public from_user_input_index_exists(string userInput)
        {
            given_file("c:\\middle-earth\\index.wraplist", "<package-list />");

            when_detecting(userInput);
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