using System;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using NUnit.Framework;
using OpenFileSystem.IO;
using OpenWrap;
using OpenWrap.Commands;
using OpenWrap.Commands.contexts;
using OpenWrap.Commands.Wrap;
using OpenWrap.PackageModel.Serialization;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace init_wrap_specs
{

    class init_wrap_in_complete_dot_folder : context.init_wrap
    {
        public init_wrap_in_complete_dot_folder()
        {
            given_current_directory(@"c:\newpackage");
            given_project_repository(new FolderRepository(Environment.CurrentDirectory.GetDirectory("wraps"), FolderRepositoryOptions.AnchoringEnabled));

            given_csharp_project_file(@"c:\newpackage\src\project1\project1.csproj");
            when_executing_command(".", "-all");
            Environment.ProjectRepository.RefreshPackages();
        }
        [Test]
        public void command_is_successful()
        {
            Results.ShouldHaveNoError();
        }
        [Test]
        public void descriptor_should_have_correct_name()
        {
            Environment.CurrentDirectory
                    .GetFile("newpackage.wrapdesc")
                    .Exists.ShouldBeTrue();
        }
        [Test]
        public void descriptor_should_have_openwrap_as_content_dependency()
        {
            new PackageDescriptorReaderWriter()
                .Read(Environment.CurrentDirectory.GetFile("newpackage.wrapdesc"))
                .Dependencies
                .FirstOrDefault(x => x.Name.EqualsNoCase("openwrap"))
                .ShouldNotBeNull()
                .ContentOnly.ShouldBeTrue();
        }
        [Test]
        public void openwrap_package_is_installed()
        {
            Environment.ProjectRepository.PackagesByName["openwrap"]
                    .ShouldHaveCountOf(1);

        }

        [Test]
        public void project_is_patched()
        {
            var projectFIle = Environment.CurrentDirectory.GetFile(@"project1.csproj");
            using (var stream = projectFIle.OpenRead())
            {
                var doc = XDocument.Load(new XmlTextReader(stream));

                doc.Document.Descendants(XName.Get("Import", MSBUILD_NS)).First().Attribute("Project").Value
                    .ShouldContain("OpenWrap.CSharp.targets");
            }
        }
    }
    class init_wrap_in_empty_dot_folder : context.init_wrap
    {
        public init_wrap_in_empty_dot_folder()
        {
            given_current_directory(@"c:\newpackage");
            given_project_repository(new FolderRepository(Environment.CurrentDirectory.GetDirectory("wraps"), FolderRepositoryOptions.AnchoringEnabled));
            when_executing_command(".");
            Environment.ProjectRepository.RefreshPackages();
        }
        [Test]
        public void command_is_successful()
        {
            Results.ShouldHaveNoError();
        }
        [Test]
        public void descriptor_should_have_correct_name()
        {
            Environment.CurrentDirectory
                    .GetFile("newpackage.wrapdesc")
                    .Exists.ShouldBeTrue();
        }
        [Test]
        public void descriptor_should_have_openwrap_as_content_dependency()
        {
            new PackageDescriptorReaderWriter()
                .Read(Environment.CurrentDirectory.GetFile("newpackage.wrapdesc"))
                .Dependencies
                .FirstOrDefault(x=>x.Name.EqualsNoCase("openwrap"))
                .ShouldNotBeNull()
                .ContentOnly.ShouldBeTrue();
        }
        [Test]
        public void openwrap_package_is_installed()
        {
            Environment.ProjectRepository.PackagesByName["openwrap"]
                    .ShouldHaveCountOf(1);
            
        }
        [Test]
        public void openwrap_junction_is_installed()
        {
            Environment.CurrentDirectory.GetDirectory("wraps").GetDirectory("openwrap")
                    .Exists.ShouldBeTrue();
        }
    }
    class init_dot_in_existing_package : context.init_wrap
    {
        public init_dot_in_existing_package()
        {
            given_current_directory(@"c:\newpackage");
            given_dependency("depends: openwrap");
            given_dependency_file("newpackage.wrapdesc");
            given_project_package("openwrap", "1.0.0");
            when_executing_command(".");
        }

        [Test]
        public void dependency_is_not_updated()
        {
            new PackageDescriptorReaderWriter()
                .Read(Environment.CurrentDirectory.GetFile("newpackage.wrapdesc"))
                .Dependencies.SingleOrDefault(x => x.Name.EqualsNoCase("openwrap"))
                    .ContentOnly.ShouldBeFalse();
        }

        void given_dependency_file(string name)
        {
            new PackageDescriptorReaderWriter()
                    .Write(
                            Environment.Descriptor,
                            Environment.CurrentDirectory.GetFile(name).OpenWrite());
        }
    }

    public class init_for_all_projects : context.init_wrap
    {
        public init_for_all_projects()
        {
            given_csharp_project_file(@"project1.csproj");
            when_executing_command("-all");
        }
        [Test]
        public void command_succeeds()
        {
            Results.ShouldHaveNoError();
        }
        [Test]
        public void project_is_patched()
        {
            var projectFIle = Environment.CurrentDirectory.GetFile(@"project1.csproj");
            using (var stream = projectFIle.OpenRead())
            {
                var doc = XDocument.Load(new XmlTextReader(stream));

                doc.Document.Descendants(XName.Get("Import", MSBUILD_NS)).First().Attribute("Project").Value
                    .ShouldContain("OpenWrap.CSharp.targets");
            }
        }

    }
    namespace context
    {
        public abstract class init_wrap : command_context<InitWrapCommand>
        {
            public init_wrap()
            {
                given_system_package("sharpziplib", "0.85.1");
                given_system_package("openfilesystem", "1.0.0");
                given_system_package("openwrap", "1.0.0", "depends: openfilesystem", "depends: sharpziplib");                
            }
            protected const string MSBUILD_NS = "http://schemas.microsoft.com/developer/msbuild/2003";

            protected void given_csharp_project_file(string projectPath)
            {
                var file = Environment.CurrentDirectory.GetFile(projectPath);
                using (var fileStream = file.OpenWrite())
                {
                    var xmlDoc = Encoding.UTF8.GetBytes(
                            @"
<Project ToolsVersion=""3.5"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
    <Import Project=""$(MSBuildToolsPath)\Microsoft.CSharp.targets"" />
</Project>
");
                    fileStream.Write(xmlDoc, 0, xmlDoc.Length);
                }
            }
        }
    }
}
