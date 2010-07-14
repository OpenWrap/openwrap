using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenFileSystem.IO.FileSystem.Local;
using OpenWrap.Dependencies;
using OpenFileSystem.IO;
using OpenWrap.Repositories;
using OpenWrap.Testing;
using OpenWrap.Tests.Slow;

namespace OpenWrap.Repositories.Wrap.Tests.Slow
{
    public class when_accessing_repositories_with_zip_files : context.folder_based_repository
    {
        public when_accessing_repositories_with_zip_files()
        {
            given_folder_repository();
            when_reading_test_module_descriptor();
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
            Descriptor.Version.ShouldBe(new Version("1.0.0"));
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
            RepositoryPath.GetDirectory("cache").GetDirectory("test-module-1.0.0")
                .Exists.ShouldBeFalse();
        }
    }
    public class when_loading_zipped_package : context.folder_based_repository
    {

        public when_loading_zipped_package()
        {
            given_folder_repository();
            when_reading_test_module();

        }

        [Test]
        public void cache_is_created()
        {
            RepositoryPath.GetDirectory("cache").GetDirectory("test-module-1.0.0")
                .Exists.ShouldBeTrue();
            
        }
        protected void when_reading_test_module()
        {
            when_reading_test_module_descriptor();
            var dependency = Descriptor.Load();

        }
    }
    namespace context
    {
        public class folder_based_repository : IDisposable
        {
            protected ITemporaryDirectory RepositoryPath;
            protected FolderRepository Repository;
            protected IPackageInfo Descriptor;
            protected WrapDependency Dependency;
            protected IFileSystem FileSystem;

            protected void given_folder_repository()
            {
                FileSystem = LocalFileSystem.Instance;
                RepositoryPath = FileSystem.CreateTempDirectory();
                var wrapFile = TestFiles.test_module_1_0_0;
                using(var file = RepositoryPath.GetFile("test-module-1.0.0.wrap").OpenWrite())
                {
                    file.Write(wrapFile,0, wrapFile.Length);
                    file.Flush();
                }
                Repository = new FolderRepository(RepositoryPath);
            }

            protected void when_reading_test_module_descriptor()
            {
                Descriptor = Repository.PackagesByName["test-module"].FirstOrDefault();

                Dependency = Descriptor.Dependencies.FirstOrDefault();
            }
            public void Dispose()
            {
                RepositoryPath.Dispose();
            }
        }
    }
}
