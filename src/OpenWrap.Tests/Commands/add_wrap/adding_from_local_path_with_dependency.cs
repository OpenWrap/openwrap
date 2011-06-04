using NUnit.Framework;
using OpenWrap.Commands.contexts;
using OpenWrap.Commands.Wrap;
using OpenWrap.Repositories;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace Tests.Commands.add_wrap
{
    class adding_from_local_path_with_dependency : command<AddWrapCommand>
    {
        public adding_from_local_path_with_dependency()
        {
            given_project_repository(new InMemoryRepository("Project repository"));

            given_currentdirectory_package("sauron", "1.0.0", "depends: one-ring");
            given_system_package("one-ring", "1.0.0");

            when_executing_command("sauron");
        }

        [Test]
        public void package_is_installed()
        {
            Environment.ProjectRepository.PackagesByName["sauron"].ShouldHaveCountOf(1);
        }

        [Test]
        public void package_dependency_is_installed()
        {
            Environment.ProjectRepository.PackagesByName["one-ring"].ShouldHaveCountOf(1);

        }
    }
}