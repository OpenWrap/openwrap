using NUnit.Framework;
using OpenWrap;
using OpenWrap.Commands.Wrap;
using OpenWrap.Repositories;
using Tests.Commands.contexts;
using Tests.Commands.update_wrap;

namespace Tests.Commands.add_wrap
{
    class adding_from_system_with_outdated_version_in_remote : contexts.add_wrap
    {
        public adding_from_system_with_outdated_version_in_remote()
        {
            given_project_repository(new InMemoryRepository("Project repository"));
            given_project_package("sauron", "1.0.0.0");
            given_system_package("sauron", "1.0.0.2");
            given_remote_package("sauron", "1.0.0.1".ToVersion());

            when_executing_command("sauron");
        }
        [Test]
        public void latest_version_of_package_is_added()
        {
            Environment.ProjectRepository.ShouldHavePackage("sauron", "1.0.0.2");
        }
        [Test]
        public void outdated_version_is_not_added()
        {
            Environment.ProjectRepository.ShouldNotHavePackage("sauron", "1.0.0.1");
        }
    }
}