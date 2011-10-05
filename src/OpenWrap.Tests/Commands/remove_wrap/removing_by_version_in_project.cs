using System.Linq;
using NUnit.Framework;
using OpenWrap.Commands;
using OpenWrap.Testing;
using Tests.Commands.update_wrap.project;

namespace Tests.Commands.remove_wrap
{
    public class removing_by_version_in_project : global::Tests.Commands.contexts.remove_wrap
    {
        public removing_by_version_in_project()
        {
            given_dependency("depends: saruman");
            given_project_package("saruman", "1.0.0.0");
            given_project_package("saruman", "1.0.0.1");
            when_executing_command("saruman -project -version 1.0.0.0");
        }

        [Test]
        public void command_succeeds()
        {
            Results.ShouldHaveNoError();
        }
        [Test]
        public void version_is_removed()
        {
            Environment.ProjectRepository.ShouldNotHavePackage("saruman", "1.0.0.0");
        }
        [Test]
        public void other_versions_are_not_removed()
        {
            Environment.ProjectRepository.ShouldHavePackage("saruman", "1.0.0.1");
        }
        [Test]
        public void descriptor_is_not_updated()
        {
            Environment.Descriptor.Dependencies.ShouldHaveCountOf(1);
        }
        [Test]
        public void warning_is_issued_about_descriptor_not_updated()
        {
            Results.Where(x=>x.Type == CommandResultType.Warning).ShouldHaveCountOf(1);
        }
    }
}
