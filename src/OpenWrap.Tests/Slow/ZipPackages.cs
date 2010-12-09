﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenFileSystem.IO.FileSystems.InMemory;
using OpenFileSystem.IO.FileSystems.Local;
using OpenRasta.Wrap.Tests.Dependencies.context;
using OpenWrap.Dependencies;
using OpenFileSystem.IO;
using OpenWrap.Repositories;
using OpenWrap.Testing;


namespace OpenWrap.Repositories.Wrap.Tests.Slow
{
    public class package_name_case_sensitivity : context.folder_based_repository
    {
        public package_name_case_sensitivity()
        {
            given_folder_repository_with_module("test-package");
            when_reading_test_module_descriptor("Test-Package");
        }
        [Test]
        public void package_is_found_case_insensitively()
        {
            Descriptor.ShouldNotBeNull();
        }
    }
    public class when_accessing_repositories_with_zip_files : context.folder_based_repository
    {
        public when_accessing_repositories_with_zip_files()
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
            RepositoryPath.GetDirectory("_cache").GetDirectory("test-module-1.0.0")
                .Exists.ShouldBeFalse();
        }
    }
    public class when_loading_zipped_package : context.folder_based_repository
    {

        public when_loading_zipped_package()
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
    namespace context
    {
        public class folder_based_repository : IDisposable
        {
            protected ITemporaryDirectory RepositoryPath;
            protected FolderRepository Repository;
            protected IPackageInfo Descriptor;
            protected PackageDependency Dependency;
            protected IFileSystem FileSystem;

            protected void given_folder_repository_with_module(string packageName)
            {
                FileSystem = LocalFileSystem.Instance;
                RepositoryPath = FileSystem.CreateTempDirectory();
                PackageBuilder.NewWithDescriptor(
                    RepositoryPath.GetFile(packageName + "-1.0.0.wrap"), 
                    packageName,
                    "1.0.0",
                    "depends: nhibernate-core = 2.1"
                    );

                Repository = new FolderRepository(RepositoryPath);
            }

            protected void when_reading_test_module_descriptor(string packageName)
            {
                Descriptor = Repository.PackagesByName[packageName].FirstOrDefault();

                Dependency = Descriptor.Dependencies.FirstOrDefault();
            }
            public void Dispose()
            {
                RepositoryPath.Dispose();
            }
        }
    }
}
