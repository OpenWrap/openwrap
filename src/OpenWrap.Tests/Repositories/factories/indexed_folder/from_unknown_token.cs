using NUnit.Framework;
using OpenWrap.Testing;
using Tests.Repositories.contexts;

namespace Tests.Repositories.factories.indexed_folder
{
    public class from_unknown_token : indexed_folder_repository
    {
        public from_unknown_token()
        {
            when_building_from_token("[unknown]http://middle.earth/index.wraplist");
        }

        [Test]
        public void repository_is_built()
        {
            Repository.ShouldBeNull();
        }
    }
}