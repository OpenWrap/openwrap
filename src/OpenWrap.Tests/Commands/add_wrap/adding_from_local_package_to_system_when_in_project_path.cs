using System;
using NUnit.Framework;
using OpenWrap.Commands.contexts;
using OpenWrap.Commands.Wrap;
using OpenWrap.Repositories;

namespace OpenWrap.Tests.Commands
{
    class adding_from_local_package_to_system_when_in_project_path : command_context<AddWrapCommand>
    {
        string SAURON_NAME = "sauron";
        Version SAURON_VERSION = new Version(1, 0, 0);

        public adding_from_local_package_to_system_when_in_project_path()
        {
            given_dependency("depends: sauron");
            given_project_repository(new InMemoryRepository("Project repository"));
            given_currentdirectory_package(SAURON_NAME, SAURON_VERSION);


            when_executing_command("-Name", SAURON_NAME, "-System");
        }
        [Test]
        public void installs_package_in_system_repository()
        {
            package_is_in_repository(Environment.SystemRepository, SAURON_NAME, SAURON_VERSION);
        }

        [Test]
        public void doesnt_install_package_in_project_repository()
        {
            package_is_not_in_repository(Environment.ProjectRepository, SAURON_NAME, SAURON_VERSION);
        }
    }
}