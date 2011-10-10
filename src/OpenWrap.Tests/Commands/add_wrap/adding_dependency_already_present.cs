using NUnit.Framework;
using OpenWrap.Commands.Wrap;
using OpenWrap.PackageManagement;
using OpenWrap.Testing;
using Tests.Commands.contexts;
using Tests.Commands.update_wrap;
using Tests.Commands.update_wrap.project;

namespace Tests.Commands.add_wrap
{
    class adding_dependency_already_present : command<AddWrapCommand>
    {
        public adding_dependency_already_present()
        {
            given_dependency("depends: sauron >= 2.0");
            given_project_package("sauron", "1.0.0");
            given_system_package("sauron", "2.0.0");

            when_executing_command("sauron");
        }

        [Test]
        public void command_fails()
        {
            Results.ShouldHaveOne<PackageDependencyAlreadyExists>()
                .PackageName.ShouldBe("sauron");
        }
        [Test]
        public void package_is_not_updated()
        {
            Environment.ProjectRepository.ShouldHavePackage("sauron", "1.0.0");
        }
    }
}