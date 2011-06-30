using NUnit.Framework;
using OpenWrap.Testing;
using Tests.Repositories.contexts;

namespace Tests.Repositories.factories.indexed_folder
{
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