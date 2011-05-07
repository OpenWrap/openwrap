using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.Repositories.factories
{
    internal class from_user_input_index_does_not_exist : contexts.simple_index_repository_factory
    {
        public from_user_input_index_does_not_exist()
        {
            when_detecting("http://middle.earth");
        }

        [Test]
        public void repository_is_built()
        {
            Repository.ShouldBeNull();
        }
    }
}   