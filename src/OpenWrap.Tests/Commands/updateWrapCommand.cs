using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenRasta.Wrap.Tests.Dependencies.context;
using OpenWrap.Build.Services;
using OpenWrap.Commands;
using OpenWrap.Commands.Wrap;
using OpenWrap.Dependencies;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace OpenWrap.Tests.Commands
{
    class when_updating_wrap_in_project_folder : context.command_context<SyncWrapCommand>
    {
        public when_updating_wrap_in_project_folder()
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
            Environment.UserRepository.PackagesByName["goldberry"].ShouldHaveCountOf(2);
        }
        [Test]
        public void the_package_is_installed_in_user_repo()
        {
            Environment.UserRepository.PackagesByName["goldberry"].Last().Version.ShouldBe(new Version(2, 2, 0));
        }
        [Test]
        public void the_package_is_installed_in_project_repo()
        {
            Environment.ProjectRepository.PackagesByName["goldberry"].Last().Version.ShouldBe(new Version(2, 2, 0));
        }

    }
}
