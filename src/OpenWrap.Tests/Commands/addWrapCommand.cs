using System;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenRasta.Wrap.Tests.Dependencies.context;
using OpenWrap.Commands.Wrap;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace OpenWrap.Tests.Commands
{
    class adding_wrap_with_incompatible_arguments : context.command_context<AddWrapCommand>
    {
        public adding_wrap_with_incompatible_arguments()
        {
            given_project_repository();

            when_executing_command("-System", "-Project");
        }
    }
    class adding_wrap_from_local_package_in_project_path : context.command_context<AddWrapCommand>
    {
        public adding_wrap_from_local_package_in_project_path()
        {
            given_dependency("depends sauron");
            given_project_repository();
            given_currentdirectory_package("sauron", new Version(1, 0, 0));


            when_executing_command("-Name", "sauron");
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

    class adding_wrap_from_local_package_in_project_path_with_system_parameter : context.command_context<AddWrapCommand>
    {
        string SAURON_NAME = "sauron";
        Version SAURON_VERSION = new Version(1, 0, 0);

        public adding_wrap_from_local_package_in_project_path_with_system_parameter()
        {
            given_dependency("depends sauron");
            given_project_repository();
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
    class adding_wrap_from_local_package_in_project_path_with_project_only_parameter : context.command_context<AddWrapCommand>
    {
        string SAURON_NAME = "sauron";
        Version SAURON_VERSION = new Version(1, 0, 0);
        public adding_wrap_from_local_package_in_project_path_with_project_only_parameter()
        {
            given_currentdirectory_package(SAURON_NAME, SAURON_VERSION);
            given_project_repository();

            when_executing_command("-Name", "sauron", "-Project");
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
    class adding_wrap_from_local_package_outside_of_project_path : context.command_context<AddWrapCommand>
    {
        public adding_wrap_from_local_package_outside_of_project_path()
        {
            given_currentdirectory_package("sauron", new Version(1, 0, 0));

            when_executing_command("-Name", "sauron");
        }
        [Test]
        public void installs_package_in_system_repository()
        {
            Environment.SystemRepository.PackagesByName["sauron"]
                .ShouldHaveCountOf(1)
                .First().Version.ShouldBe(new Version(1, 0, 0));
        }
        [Test]
        public void command_is_successful()
        {
            Results.ShouldHaveAll(x => x.Success);
        }
    }

    class adding_non_existant_wrap : context.command_context<AddWrapCommand>
    {
        public adding_non_existant_wrap()
        {
            given_currentdirectory_package("sauron", new Version(1, 0, 0));
            when_executing_command("-Name", "saruman");
        }
        [Test]
        public void package_installation_is_unsuccessfull()
        {
            Results.ShouldHaveAtLeastOne(x => x.Success == false);
        }
    }
}