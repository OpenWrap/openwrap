using System;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Commands.Wrap;
using OpenWrap.Repositories;
using Tests.Commands.contexts;

namespace Tests.Commands.add_wrap
{
    class adding_wrap_from_local_package_in_project_path_with_project_only_parameter : command<AddWrapCommand>
    {
        public adding_wrap_from_local_package_in_project_path_with_project_only_parameter()
        {
            given_currentdirectory_package("sauron", "1.0.0".ToSemVer());
            given_project_repository(new InMemoryRepository("Project repository"));

            when_executing_command("-Name sauron -Project");
        }
        [Test]
        public void installs_package_in_project_repository()
        {
            package_is_in_repository(Environment.ProjectRepository, "sauron", "1.0.0".ToSemVer());
        }
        [Test]
        public void doesnt_install_package_in_system_repository()
        {
            package_is_not_in_repository(Environment.SystemRepository, "sauron", "1.0.0".ToSemVer());
        }
    }
}