using System;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Commands.Wrap;
using OpenWrap.Repositories;
using Tests.Commands.contexts;

namespace Tests.Commands.add_wrap
{
    class adding_from_local_package_to_system_when_in_project_path : command<AddWrapCommand>
    {
        public adding_from_local_package_to_system_when_in_project_path()
        {
            given_dependency("depends: sauron");
            given_project_repository(new InMemoryRepository("Project repository"));
            given_currentdirectory_package("sauron", "1.0.0");


            when_executing_command("-Name " + "sauron" + " -System");
        }
        [Test]
        public void installs_package_in_system_repository()
        {
            package_is_in_repository(Environment.SystemRepository, "sauron", "1.0.0".ToSemVer());
        }

        [Test]
        public void doesnt_install_package_in_project_repository()
        {
            package_is_not_in_repository(Environment.ProjectRepository, "sauron", "1.0.0".ToSemVer());
        }
    }
}