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
    public class when_updating_packages_in_project_folder : context.command_context<UpdateWrapCommand>
    {
        public when_updating_packages_in_project_folder()
        {
            given_dependency("depends goldberry >= 2.0");

            given_project_package("goldberry", new Version(2, 0, 0));
            given_user_package("goldberry", new Version(2,1,0));
            given_remote_package("goldberry", new Version(2, 2, 0));

            when_executing_command();
        }

        [Test]
        public void the_package_is_installed_alongside_previous_version_in_user_repo()
        {
            Environment.SystemRepository.PackagesByName["goldberry"].ShouldHaveCountOf(2);
        }

        [Test]
        public void the_package_is_installed_in_user_repo()
        {
            Environment.SystemRepository.PackagesByName["goldberry"].Last().Version.ShouldBe(new Version(2, 2, 0));
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
            given_user_package("goldberry", new Version(2,0,0));
            given_remote_package("goldberry", new Version(2, 1, 0));

            when_executing_command();
        }
        [Test]
        public void results_are_successful()
        {
            Results.All(x => x.Success).ShouldBeTrue();
        }
        [Test]
        public void package_in_user_repository_is_updated()
        {
            Environment.SystemRepository.ShouldHavePackage("goldberry", "2.1.0");
        }
    }
    public static class RepositoryAssertions
    {
        public static T ShouldHavePackage<T>(this T repository, string name, string version)
            where T:IPackageRepository
        {
            repository.PackagesByName[name].Count().ShouldBeGreaterThan(0);
            repository.HasDependency(name, new Version(version)).ShouldBeTrue();
            return repository;
        }
    }
}
