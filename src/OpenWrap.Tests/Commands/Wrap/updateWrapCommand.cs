using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenRasta.Wrap.Tests.Dependencies.context;
using OpenWrap.Commands;
using OpenWrap.Commands.Wrap;
using OpenWrap.Dependencies;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace OpenWrap.Tests.Commands
{
    public class update_package_not_existing_anywhere_but_in_project : context.command_context<UpdateWrapCommand>
    {
        public update_package_not_existing_anywhere_but_in_project()
        {
            given_dependency("depends: goldberry");
            given_project_package("goldberry", "1.0");

            when_executing_command();
        }
        [Test]
        public void dependency_not_found_warning_is_produced()
        {
            Results.OfType<DependenciesNotFoundInRepositories>()
                    .ShouldHaveCountOf(1)
                    .First().Dependencies
                    .ShouldHaveCountOf(1).First().Dependency.Name.ShouldBe("goldberry");
        }
        [Test]
        public void no_error_should_be_reported()
        {
            Results.ShouldHaveNo(x => x.Error());
        }
    }
    public class update_package_by_name_in_project : context.command_context<UpdateWrapCommand>
    {
        public update_package_by_name_in_project()
        {
            given_dependency("depends: goldberry = 2");
            given_dependency("depends: one-ring = 1");
            given_project_package("goldberry", "2.0");
            given_project_package("one-ring", "1.0");
            given_remote_package("one-ring", "1.1");
            given_remote_package("goldberry", "2.1");

            when_executing_command("one-ring", "-project");
        }
        [Test]
        public void project_is_updated()
        {
            Environment.ProjectRepository.ShouldHavePackage("one-ring", "1.1");
        }
        [Test]
        public void not_selected_projects_are_not_updated()
        {
            Environment.ProjectRepository.ShouldNotHavePackage("goldberry", "2.1");
        }
    }
    public class update_package_by_name_in_system : context.command_context<UpdateWrapCommand>
    {
        public update_package_by_name_in_system()
        {
            given_system_package("goldberry", "2.0");
            given_system_package("one-ring", "1.0");
            given_remote_package("one-ring", "1.1");
            given_remote_package("goldberry", "2.1");

            when_executing_command("one-ring", "-sys");
        }
        [Test]
        public void project_is_updated()
        {
            Environment.SystemRepository.ShouldHavePackage("one-ring", "1.1");
        }
        [Test]
        public void not_selected_projects_are_not_updated()
        {
            Environment.SystemRepository.ShouldNotHavePackage("goldberry", "2.1");
        }
    }
    public class when_updating_packages_in_project_folder : context.command_context<UpdateWrapCommand>
    {
        public when_updating_packages_in_project_folder()
        {
            given_dependency("depends: goldberry >= 2.0");

            given_project_package("goldberry", "2.0.0");
            given_system_package("goldberry", "2.1.0");
            given_remote_package("goldberry", "2.2.0");

            when_executing_command();
        }

        [Test]
        public void the_package_is_not_installed_in_system_repo()
        {
            Environment.SystemRepository.PackagesByName["goldberry"].Last().Version.ShouldBe(new Version(2, 1, 0));
        }

        [Test]
        public void the_package_is_installed_in_project_repo()
        {
            Environment.ProjectRepository.PackagesByName["goldberry"].Last().Version.ShouldBe(new Version(2, 2, 0));
        }
    }
    public class when_not_in_project_folder_and_package_can_be_updated : context.command_context<UpdateWrapCommand>
    {
        public when_not_in_project_folder_and_package_can_be_updated()
        {
            given_system_package("goldberry", "2.0.0");
            given_remote_package("goldberry", "2.1.0");

            when_executing_command();
        }
        [Test]
        public void error_message_is_generated()
        {
            Results.Any(x => x.Success() == false).ShouldBeTrue();
        }
        [Test]
        public void package_in_system_repository_is_not_updated()
        {
            Environment.SystemRepository.ShouldHavePackage("goldberry", "2.0.0");
        }
    }
    public class system_flag_is_specified : context.command_context<UpdateWrapCommand>
    {
        public system_flag_is_specified()
        {
            given_project_package("goldberry", "2.0.0");
            given_system_package("goldberry", "2.1.0");

            when_executing_command("-system");
        }
        [Test]
        public void project_repo_not_updated()
        {
            Environment.ProjectRepository.ShouldHavePackage("goldberry","2.0.0");
        }
    }
    public class project_and_system_defaults : context.command_context<UpdateWrapCommand>
    {
        UpdateWrapCommand CommandInstance;

        public project_and_system_defaults()
        {
            CommandInstance = new UpdateWrapCommand();
        }
        [Test]public void project_is_selected()
        {
            CommandInstance.Project.ShouldBeTrue();
        }
        [Test]
        public void system_is_not_selected()
        {
            CommandInstance.System.ShouldBeFalse();
        }
    }
    public class project_is_selected_system_isnt : context.command_context<UpdateWrapCommand>
    {
        UpdateWrapCommand CommandInstance;

        public project_is_selected_system_isnt()
        {
            CommandInstance = new UpdateWrapCommand() { Project = true};
        }
        [Test]
        public void project_is_selected()
        {
            CommandInstance.Project.ShouldBeTrue();
        }
        [Test]
        public void system_is_not_selected()
        {
            CommandInstance.System.ShouldBeFalse();
        }
    }
    public class project_is_not_selected_system_is : context.command_context<UpdateWrapCommand>
    {
        UpdateWrapCommand CommandInstance;

        public project_is_not_selected_system_is()
        {
            CommandInstance = new UpdateWrapCommand() { System = true };
        }
        [Test]
        public void project_is_selected()
        {
            CommandInstance.Project.ShouldBeFalse();
        }
        [Test]
        public void system_is_not_selected()
        {
            CommandInstance.System.ShouldBeTrue();
        }
    }
    public class project_and_system_flags_specified : context.command_context<UpdateWrapCommand>
    {
        
        public project_and_system_flags_specified()
        {
            given_dependency("depends: goldberry");


            given_project_package("goldberry", "2.0.0");
            given_system_package("goldberry", "2.0.0");
            given_remote_package("goldberry", "3.0.0");


            when_executing_command("-system","-project");
        }
        [Test]
        public void project_repo_updated()
        {
            Environment.ProjectRepository.ShouldHavePackage("goldberry", "3.0.0");
        }
        [Test]
        public void system_repo_updated()
        {
            Environment.SystemRepository.ShouldHavePackage("goldberry", "3.0.0");
            
        }
    }
    public static class RepositoryAssertions
    {
        public static T ShouldHavePackage<T>(this T repository, string name, string version)
            where T : IPackageRepository
        {
            repository.PackagesByName[name].Count().ShouldBeGreaterThan(0);
            repository.HasPackage(name, version).ShouldBeTrue();
            return repository;
        }
        public static T ShouldNotHavePackage<T>(this T repository, string name, string version)
            where T : IPackageRepository
        {
            repository.HasPackage(name, version).ShouldBeFalse();
            return repository;
        }
    }
}
