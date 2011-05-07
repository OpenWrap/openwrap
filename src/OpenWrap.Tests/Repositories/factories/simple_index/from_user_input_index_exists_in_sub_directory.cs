using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.Repositories.factories
{
    [TestFixture("http://middle.earth/rohan")]
    [TestFixture("http://middle.earth/rohan/")]
    public class from_user_input_index_exists_in_sub_directory : contexts.simple_index_repository_factory
    {
        public from_user_input_index_exists_in_sub_directory(string userInput)
        {
            given_remote_resource("http://middle.earth/rohan/index.wraplist", "application/vnd.openwrap.index+xml", "<package-list />");
            when_detecting(userInput);
        }

        [Test]
        public void repository_is_built()
        {
            Repository.ShouldNotBeNull();
        }
    }
}