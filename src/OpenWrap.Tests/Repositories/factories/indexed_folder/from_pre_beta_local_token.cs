using NUnit.Framework;
using OpenWrap.Repositories.FileSystem;
using OpenWrap.Testing;
using Tests.Repositories.contexts;

namespace Tests.Repositories.factories.indexed_folder
{
    public class from_pre_beta_local_token : indexed_folder_repository
    {
        public from_pre_beta_local_token()
        {
            when_building_from_token("[indexed-folder]c:\\folder\\index.wraplist");
        }

        [Test]
        public void repository_is_built()
        {
            Repository.ShouldNotBeNull()
                .ShouldBeOfType<IndexedFolderRepository>()
                .Directory.ShouldBe(FileSystem.GetDirectory("c:\\folder\\"));
        }

        [Test]
        public void token_is_converted_to_uri()
        {
            Repository.Token.ShouldBe("[indexed-folder]indexed-folder:///c:/folder/index.wraplist");
        }
    }
}