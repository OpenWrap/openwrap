using System;
using NUnit.Framework;
using OpenWrap.Commands.contexts;
using OpenWrap.Commands.Wrap;
using OpenWrap.Repositories;
using Tests.Commands.contexts;

namespace Tests.Commands.add_wrap
{
    class adding_wrap_from_local_package_in_project_path_with_project_only_parameter : command<AddWrapCommand>
    {
        string SAURON_NAME = "sauron";
        Version SAURON_VERSION = new Version(1, 0, 0);
        public adding_wrap_from_local_package_in_project_path_with_project_only_parameter()
        {
            given_currentdirectory_package(SAURON_NAME, SAURON_VERSION);
            given_project_repository(new InMemoryRepository("Project repository"));

            when_executing_command("-Name sauron -Project");
        }
        [Test]
        public void installs_package_in_project_repository()
        {
            package_is_in_repository(Environment.ProjectRepository, SAURON_NAME, SAURON_VERSION);
        }
        [Test]
        public void doesnt_install_package_in_system_repository()
        {
            package_is_not_in_repository(Environment.SystemRepository, SAURON_NAME, SAURON_VERSION);
        }
    }
}