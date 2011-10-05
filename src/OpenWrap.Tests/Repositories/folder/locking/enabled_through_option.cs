using NUnit.Framework;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests.Repositories.folder.locking
{
    public class enabled_through_option : contexts.folder
    {
        public enabled_through_option()
        {
            given_folder_repository(FolderRepositoryOptions.SupportLocks);
        }

        [Test]
        public void feature_is_supported()
        {
            repository.Feature<ISupportLocking>().ShouldNotBeNull();
        }
    }
}