using System;
using System.IO;
using NUnit.Framework;
using OpenWrap.Testing;
using Tests.Repositories.context;

namespace Tests.Repositories
{
    public class publishing_package : current_directory_repository
    {
        public publishing_package()
        {
            given_current_folder_repository();
        }

        [Test]
        public void attempting_publish_results_in_error()
        {
            Executing(() => Repository.Publish("isengard", new MemoryStream()))
                .ShouldThrow<NotSupportedException>();
        }

        [Test]
        public void publish_is_disabled()
        {
            Repository.CanPublish.ShouldBeFalse();
        }
    }
}