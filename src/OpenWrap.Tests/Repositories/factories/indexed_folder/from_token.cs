using NUnit.Framework;
using OpenWrap.Testing;
using Tests.Repositories.contexts;

namespace Tests.Repositories.factories.indexed_folder
{
    public class from_token : indexed_folder_repository
    {
        public from_token()
        {
            when_building_from_token("[indexed-folder]c:\\folder\\index.wraplist");
        }

        [Test]
        public void repository_is_built()
        {
            Repository.ShouldNotBeNull()
                .Directory.ShouldBe(FileSystem.GetDirectory("c:\\folder\\"));
        }
    }
}