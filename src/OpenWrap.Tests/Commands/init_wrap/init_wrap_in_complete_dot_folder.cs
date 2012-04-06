using System.Linq;
using System.Xml;
using System.Xml.Linq;
using NUnit.Framework;
using OpenFileSystem.IO;
using OpenWrap;
using OpenWrap.PackageModel;
using OpenWrap.PackageModel.Serialization;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests.Commands.init_wrap
{

    class init_wrap_in_complete_dot_folder : contexts.init_wrap
    {
        public init_wrap_in_complete_dot_folder()
        {
            given_current_directory(@"c:\newpackage");
            given_project_repository(new FolderRepository(Environment.CurrentDirectory.GetDirectory("wraps"), FolderRepositoryOptions.AnchoringEnabled));

            given_csharp_project_file(@"c:\newpackage\src\project1\project1.csproj");
            when_executing_command(". -all");
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
            new PackageDescriptorReader().Read(Environment.CurrentDirectory.GetFile("newpackage.wrapdesc"))
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
}
