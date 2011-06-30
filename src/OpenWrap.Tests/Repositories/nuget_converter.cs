using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using NUnit.Framework;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.InMemory;
using OpenFileSystem.IO.FileSystems.Local;
using OpenWrap;
using OpenWrap.PackageManagement;
using OpenWrap.PackageManagement.Packages;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;
using OpenWrap.Repositories.NuGet;
using OpenWrap.Runtime;
using OpenWrap.Testing;
using OpenWrap.Tests;
using OpenWrap.Tests.Repositories;
using Tests.Repositories;
using IOPath = System.IO.Path;

namespace nuget_converter_specs
{
    public class convertings_package_from_non_seekable_stream : contexts.nuget_converter
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
    public class converting_package : contexts.nuget_converter
    {
        public converting_package()
        {
            given_nuget_package(TestFiles.TestPackageOld);
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
            package.Content.Single(x=>x.Key == "bin-net20").ShouldHaveCountOf(1)
                    .First().File.Name.ShouldBe("empty.dll");
        }
    }
    public class converting_description_with_line_breaks : contexts.nuspec

{
        public converting_description_with_line_breaks()
        {
            given_spec("one-ring", "1.0.0", "sauron, saruman", "One ring\r\nto \n\nrule \nthem all.");
            
        }  
        [Test]
        public void description_is_converted()
        {
            Descriptor.Description.ShouldBe("One ring to rule them all.");
        }
}

    public class converting_required_values : contexts.nuspec
    {
        public converting_required_values()
        {
            given_spec("one-ring", "1.0.0", "sauron, saruman", "One ring to rule them all.");
        }

        [Test]
        public void name_is_converted()
        {
            Descriptor.Name.ShouldBe("one-ring");
        }

        [Test]
        public void version_is_converted()
        {
            Descriptor.Version.ShouldBe("1.0.0".ToVersion());
        }


        //[Test]
        //public void authors_are_converted()
        //{
            
        //}
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
            return NuConverter.ConvertNuGetVersionRange(s).Select(x=>x.ToString()).JoinString(" and ");
        }
    }
    namespace contexts
    {

        public abstract class nuspec : OpenWrap.Testing.context
        {
            const string template =
                    @"<?xml version=""1.0"" encoding=""utf-8""?>
<package>
  <metadata>
    <id>{0}</id>
    <version>{1}</version>
    <authors>{2}</authors>
    <description>{3}</description>
    <title>{4}</title>
    <owners>{5}</owners>
    <projectUrl>{6}</projectUrl>
    <licenseUrl>{7}</licenseUrl>
    <requireLicenseAcceptance>{8}</requireLicenseAcceptance>
    <tags>{9}</tags>
    <dependencies>
      <dependency id=""first"" version=""3.0.0"" />
      <dependency id=""second""/>
      <dependency id=""third"" version=""[1.0.0)"" />
    </dependencies> 
  </metadata>
</package>";

            protected PackageDescriptor given_spec(string id, string version, string authors, string description, string title = null, string owners = null, string projectUrl = null, string licenseUrl = null, string requireLicenseAcceptance = null, string tags = null)
            {
                var spec = String.Format(template, id, version, authors, description, title, owners, projectUrl, licenseUrl, requireLicenseAcceptance, tags);
                var doc = new XmlDocument();
                doc.LoadXml(spec);
                return Descriptor =  NuConverter.ConvertSpecificationToDescriptor(doc);
            }

            protected PackageDescriptor Descriptor { get; set; }
        }
        public abstract class nuget_converter : OpenWrap.Testing.context
        {
            IFileSystem FileSystem;

            public nuget_converter()
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
                
                Package = new CachedZipPackage(null, wrapFile, cacheDir);
            }

            protected IPackageInfo Package { get; set; }
            protected MemoryStream NuPackage { get; set; }

            protected void given_nuget_package(byte[] testPackage)
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
