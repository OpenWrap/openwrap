using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.Repositories.folder
{
    public class accessing_repositories_with_zip_files : context.folder_based_repository
    {
        public accessing_repositories_with_zip_files()
        {
            given_folder_repository_with_module("test-module");
            when_reading_test_module_descriptor("test-module");
        }
        [Test]
        public void descirptor_is_read()
        {
            Descriptor.ShouldNotBeNull();
        }
        [Test]
        public void name_is_correct()
        {
            Descriptor.Name.ShouldBe("test-module");
        }
        [Test]
        public void version_is_correct()
        {
            Descriptor.Version.ShouldBe("1.0.0");
        }
        [Test]
        public void dependencies_are_correct()
        {
            Dependency.ShouldNotBeNull();
            Dependency.Name.ShouldBe("nhibernate-core");
            Dependency.ToString().ShouldBe("nhibernate-core = 2.1");
        }
        [Test]
        public void cache_is_not_created_yet()
        {
            RepositoryPath.GetDirectory("_cache").GetDirectory("test-module-1.0.0")
                .Exists.ShouldBeFalse();
        }
    }
}