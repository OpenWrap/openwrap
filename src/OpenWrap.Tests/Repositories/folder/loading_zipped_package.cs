using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.Repositories.folder
{
    public class loading_zipped_package : context.folder_based_repository
    {

        public loading_zipped_package()
        {
            given_folder_repository_with_module("test-module");
            when_reading_test_module();

        }

        [Test]
        public void cache_is_created()
        {
            RepositoryPath.GetDirectory("_cache").GetDirectory("test-module-1.0.0")
                .Exists.ShouldBeTrue();
            
        }
        protected void when_reading_test_module()
        {
            when_reading_test_module_descriptor("test-module");
            var dependency = Descriptor.Load();

        }
    }
}