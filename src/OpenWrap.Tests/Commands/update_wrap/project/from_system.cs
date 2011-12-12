using System;
using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Commands.Wrap;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace Tests.Commands.update_wrap.project
{
    public class from_system: contexts.update_wrap
    {
        public from_system()
        {
            given_dependency("depends: goldberry >= 2.0");

            given_project_package("goldberry", "2.0.0");
            given_system_package("goldberry", "2.1.0");

            when_executing_command();
        }

        [Test]
        public void the_package_is_installed_in_project_repo()
        {
            Environment.ProjectRepository.PackagesByName["goldberry"].Last()
                .Version.ShouldBe("2.1.0");
        }
    }
}