using System.Linq;
using NUnit.Framework;
using OpenWrap.Commands.contexts;
using OpenWrap.Testing;
using OpenWrap.Tests.Commands;
using Tests.Commands.update_wrap;

namespace OpenWrap.Commands.remove_wrap
{
    public class removing_by_version_in_project : remove_wrap_command
    {
        public removing_by_version_in_project()
        {
            given_dependency("depends: saruman");
            given_project_package("saruman", "1.0.0.0");
            given_project_package("saruman", "1.0.0.1");
            when_executing_command("saruman", "-project", "-version", "1.0.0.0");
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