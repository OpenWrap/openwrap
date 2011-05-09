using NUnit.Framework;
using OpenWrap;
using OpenWrap.Commands.contexts;
using OpenWrap.Commands.Wrap;
using OpenWrap.Tests.Commands;

namespace Tests.Commands.update_wrap.project
{
    public class from_remote_by_name : command_context<UpdateWrapCommand>
    {
        public from_remote_by_name()
        {
            given_dependency("depends: goldberry = 2");
            given_dependency("depends: one-ring = 1");
            given_project_package("goldberry", "2.0");
            given_project_package("one-ring", "1.0");
            given_remote_package("one-ring", "1.1".ToVersion());
            given_remote_package("goldberry", "2.1".ToVersion());

            when_executing_command("one-ring -project");
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
}