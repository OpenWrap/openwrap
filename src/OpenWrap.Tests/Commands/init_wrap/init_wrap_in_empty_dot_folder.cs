using System.Linq;
using NUnit.Framework;
using OpenFileSystem.IO;
using OpenWrap;
using OpenWrap.PackageModel;
using OpenWrap.PackageModel.Serialization;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests.Commands.init_wrap
{
    class init_wrap_in_empty_dot_folder : contexts.init_wrap
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
            new PackageDescriptorReader().Read(Environment.CurrentDirectory.GetFile("newpackage.wrapdesc"))
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
}