using NUnit.Framework;
using OpenWrap.Testing;
using Tests.Repositories.contexts;

namespace Tests.Repositories.factories.indexed_folder
{
    class from_user_input_index_does_not_exist : indexed_http_repository
    {
        public from_user_input_index_does_not_exist()
        {
            when_detecting("d:\\file");
        }

        [Test]
        public void repository_is_built()
        {
            Repository.ShouldBeNull();
        }
    }
}