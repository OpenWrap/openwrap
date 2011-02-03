using System;
using System.Linq;
using NUnit.Framework;
using OpenWrap.Commands.contexts;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace OpenWrap.Tests.Commands
{
    class adding_wrap_from_local_package_in_project_path_without_descriptor_update : add_wrap_command
    {
        
        public adding_wrap_from_local_package_in_project_path_without_descriptor_update()
        {
            given_project_repository(new InMemoryRepository("Project repository"));
            given_currentdirectory_package("sauron", new Version(1, 0, 0));


            when_executing_command("-Name","sauron", "-nodesc");
        }

        [Test]
        public void the_package_is_installed_on_project_repository()
        {
            Environment.ProjectRepository.PackagesByName["sauron"].ShouldHaveCountOf(1);
        }

        [Test]
        public void descriptor_is_not_updated()
        {
            SpecExtensions.ShouldBeNull<PackageDependency>(Environment.Descriptor.Dependencies.FirstOrDefault(x=>x.Name == "sauron"));
        }
        [Test]
        public void descriptor_file_is_not_updated()
        {
            SpecExtensions.ShouldBeNull<PackageDependency>(PostExecutionDescriptor.Dependencies.FirstOrDefault(x => x.Name == "sauron"));
        }
    }
}