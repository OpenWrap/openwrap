using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.Repositories.factories.nuget
{
    public class from_token : contexts.nuget_repository_factory
    {
        public from_token()
        {
            when_building_from_token("[nuget][https://go.microsoft.com/fwlink/?LinkID=206669]http://middle.earth/mordor/nuget/Packages");
        }

        [Test]
        public void repository_is_built()
        {
            Repository.ShouldNotBeNull();
        }
    }
}