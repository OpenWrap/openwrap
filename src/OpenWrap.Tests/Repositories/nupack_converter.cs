using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.InMemory;
using OpenFileSystem.IO.FileSystems.Local;
using OpenWrap;
using OpenWrap.Dependencies;
using OpenWrap.Exports;
using OpenWrap.Repositories;
using OpenWrap.Repositories.NuGet;
using OpenWrap.Testing;
using OpenWrap.Tests;
using OpenWrap.Tests.Repositories;
using IOPath = System.IO.Path;

namespace nupack_converter_specs
{
    public class converting_package_from_non_seekable_stream : context.nupack_converter
    {
        public converting_package_from_non_seekable_stream()
        {
            given_readonly_nu_package(TestFiles.TestPackage);
            when_converting_package();
        }
        [Test]
        public void package_is_converted()
        {
            Package.ShouldNotBeNull();
        }
    }
    public class converting_package : context.nupack_converter
    {
        public converting_package()
        {
            given_nupack_package(TestFiles.TestPackage);
            when_converting_package();
        }
        [Test]
        public void name_is_correct()
        {
            Package.Name.ShouldBe("TestPackage");
        }
        [Test]
        public void version_is_correct()
        {
            Package.Version.ShouldBe("1.0.0.1234".ToVersion());
        }
        [Test]
        public void exact_version_dependency_is_correct()
        {
            Package.Dependencies.First(x => x.Name == "one-ring").ToString().ShouldBe("one-ring = 1.0.0");
        }
        [Test]
        public void min_version_dependency_is_correct()
        {
            Package.Dependencies.First(x => x.Name == "shire").ToString().ShouldBe("shire >= 2.0.0");
        }
        [Test]
        public void assembly_is_in_bin_folder()
        {
            var package = Package.Load();
            var exports = package.GetExport("bin-net20", new ExecutionEnvironment { Platform = "AnyCPU", Profile = "net20" });
            exports
                    .Items.ShouldHaveCountOf(1)
                    .First().FullPath.Check(x => IOPath.GetFileName(x).ShouldBe("empty.dll"));
        }
    }
    namespace context
    {
        public abstract class nupack_converter : OpenWrap.Testing.context
        {
            IFileSystem FileSystem;

            public nupack_converter()
            {
                this.FileSystem = new InMemoryFileSystem();
                //this.FileSystem = LocalFileSystem.Instance;
            }
            protected void when_converting_package()
            {

                var cacheDir = FileSystem.GetDirectory(@"c:\cache\TestPackage-1.0.0.1234");
                var wrapFile = FileSystem.GetFile(@"c:\tmp\TestPackage-1.0.0.1234.wrap");

                //c:\tmp\TestPackage-1.0.0.1234.wrap
                using(var openWrapPackage = wrapFile.OpenWrite())
                    NuGetConverter.Convert(NuPackage, openWrapPackage);
                
                Package = new CachedZipPackage(null, wrapFile, cacheDir, new IExportBuilder[0]);
            }

            protected IPackageInfo Package { get; set; }
            protected MemoryStream NuPackage { get; set; }

            protected void given_nupack_package(byte[] testPackage)
            {
                NuPackage = new MemoryStream(testPackage);
            }

            protected void given_readonly_nu_package(byte[] testPackage)
            {
                NuPackage = new NonSeekableMemoryStream(testPackage);
            }
        }
    }
}
