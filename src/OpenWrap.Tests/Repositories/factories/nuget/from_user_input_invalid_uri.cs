using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.Repositories.factories.nuget
{
    public class from_user_input_invalid_uri : contexts.nuget_repository_factory
    {
        public from_user_input_invalid_uri()
        {
            when_detecting("*!>");
        }

        [Test]
        public void repository_is_not_constructed()
        {
            Repository.ShouldBeNull();
        }
    }
}