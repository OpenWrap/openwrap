using System;
using System.Linq;
using NUnit.Framework;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests.Commands.add_wrap
{
    class adding_wrap_from_local_package_in_project_path_without_descriptor_update : contexts.add_wrap
    {
        public adding_wrap_from_local_package_in_project_path_without_descriptor_update()
        {
            given_project_repository(new InMemoryRepository("Project repository"));
            given_currentdirectory_package("sauron", "1.0.0");

            when_executing_command("-Name sauron -nodesc");
        }

        [Test]
        public void the_package_is_installed_on_project_repository()
        {
            Environment.ProjectRepository.PackagesByName["sauron"].ShouldHaveCountOf(1);
        }

        [Test]
        public void descriptor_is_not_updated()
        {
            Environment.Descriptor.Dependencies.FirstOrDefault(x=>x.Name == "sauron").ShouldBeNull();
        }
        [Test]
        public void descriptor_file_is_not_updated()
        {
            PostExecutionDescriptor.Dependencies.FirstOrDefault(x => x.Name == "sauron").ShouldBeNull();
        }
    }
}