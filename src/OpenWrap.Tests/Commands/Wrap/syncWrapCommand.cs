using System;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenWrap.Commands.Wrap;
using OpenWrap.Testing;

namespace OpenWrap.Tests.Commands
{
    class when_synchronizing_dependencies : context.command_context<SyncWrapCommand>
    {
        public when_synchronizing_dependencies()
        {
            given_dependency("depends: rings-of-power");
            given_project_repository();
            given_remote_package("sauron", new Version(1, 0, 0));
            given_remote_package("rings-of-power", new Version(1,0,0), "depends: sauron");

            when_executing_command();
        }

        [Test]
        public void local_repository_has_new_packages()
        {

            Environment.ProjectRepository.PackagesByName["sauron"]
                .ShouldHaveCountOf(1)
                .First().Version.ShouldBe(new Version(1, 0, 0));

            Environment.ProjectRepository.PackagesByName["rings-of-power"]
                .ShouldHaveCountOf(1)
                .First().Version.ShouldBe(new Version(1, 0, 0));
        }

        [Test]
        public void user_repository_has_new_packages()
        {
            Environment.SystemRepository.PackagesByName["sauron"]
                .ShouldHaveCountOf(1)
                .First().Version.ShouldBe(new Version(1, 0, 0));

            Environment.SystemRepository.PackagesByName["rings-of-power"]
                .ShouldHaveCountOf(1)
                .First().Version.ShouldBe(new Version(1, 0, 0));
        
        }
    }
    namespace context
    {
    }
}
