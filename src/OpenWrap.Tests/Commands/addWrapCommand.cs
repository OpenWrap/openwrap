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
    class adding_wrap_from_local_package : context.command_context<AddWrapCommand>
    {
        public adding_wrap_from_local_package()
        {
            given_dependency("depends sauron");
            given_project_repository();
            given_currentdirectory_package("sauron", new Version(1, 0, 0));


            when_executing_command("-Name","sauron");
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

}
