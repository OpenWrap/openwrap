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
    public class convertings_package_from_non_seekable_stream : context.nupack_converter
    {
        public convertings_package_from_non_seekable_stream()
        {
            given_readonly_nu_package(TestFiles.TestPackageOld);
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
            given_nupack_package(TestFiles.TestPackageOld);
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
        public void exact_version_dependency_is_per_latest()
        {
            Package.Dependencies.First(x => x.Name == "one-ring").ToString().ShouldBe("one-ring >= 1.0 and < 1.1");
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
    public class maven_style_version_definition : OpenWrap.Testing.context
    {
        [Test]
        public void default_version()
        {
            version("1.0.0").ShouldBe(">= 1.0 and < 1.1");
        }

        [Test]
        public void default_version_major_minor()
        {
            version("1.0").ShouldBe("= 1.0");
        }

        public void less_than_or_equal()
        {
            version("(,1.0]").ShouldBe("<= 1.0");
        }
        public void less_than()
        {
            version("(,1.0)").ShouldBe("< 1.0");
        }

        [Test]
        public void exact_version()
        {
            version("[1.0]").ShouldBe("= 1.0");
        }

        [Test]
        public void more_than_or_equal()
        {
            version("[1.0,)").ShouldBe(">= 1.0");
        }

        [Test]
        public void more_than()
        {
            version("(1.0,)").ShouldBe("> 1.0");
        }

        [Test]
        public void less_and_less()
        {
            version("(1.0,2.0)").ShouldBe("> 1.0 and < 2.0");
        }

        [Test]
        public void less_equal_and_less_equal()
        {
            version("[1.0,2.0]").ShouldBe(">= 1.0 and <= 2.0");
        }
        string version(string s)
        {
            return NuGetConverter.ConvertNuGetVersionRange(s).Select(x=>x.ToString()).Join(" and ");
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
