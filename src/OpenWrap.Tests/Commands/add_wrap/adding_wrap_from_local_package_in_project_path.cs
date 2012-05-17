using System;
using NUnit.Framework;
using OpenWrap.Commands.Wrap;
using OpenWrap.Repositories;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace Tests.Commands.add_wrap
{
    class adding_wrap_from_local_package_in_project_path : command<AddWrapCommand>
    {
        public adding_wrap_from_local_package_in_project_path()
        {
            given_project_repository(new InMemoryRepository("Project repository"));
            given_currentdirectory_package("sauron", "1.0.0");

            when_executing_command("-Name sauron -project -system");
        }

        [Test]
        public void the_package_is_installed_on_project_repository()
        {
            Environment.ProjectRepository.PackagesByName["sauron"].ShouldHaveCountOf(1);
        }

        [Test]
        public void the_package_is_installed_on_system_repository()
        {
            Environment.SystemRepository.PackagesByName["sauron"].ShouldHaveCountOf(1);
        }
    }
}