using NUnit.Framework;
using OpenWrap.Testing;
using Tests.Repositories.contexts;

namespace Tests.Repositories.factories.indexed_folder
{
    public class from_unc_path : indexed_folder_repository
    {
        public from_unc_path()
        {
            given_file(@"\\server\share\index.wraplist", "<package-list />");
            when_detecting("file://server/share/");
        }

        [Test]
        public void repository_is_built()
        {
            Repository.ShouldNotBeNull();
        }

        [Test]
        public void token_is_generated()
        {
            Repository.Token.ShouldBe(@"[indexed-folder]\\server\share\index.wraplist");
        }
    }
}