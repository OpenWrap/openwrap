using NUnit.Framework;
using OpenWrap.Testing;
using Tests.Repositories.contexts;

namespace Tests.Repositories.factories.indexed_http
{
    class from_invalid_user_input : indexed_http_repository
    {
        public from_invalid_user_input()
        {
            when_detecting("iron-hills");
        }

        [Test]
        public void repository_is_not_built()
        {
            Repository.ShouldBeNull();
        }
    }
}