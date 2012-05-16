using NUnit.Framework;
using OpenWrap;
using Tests.Commands.update_wrap.project;

namespace Tests.Commands.update_wrap
{
    public class by_name_only_updates_affected_dependency : contexts.update_wrap
    {
        public by_name_only_updates_affected_dependency()
        {
            given_dependency("depends: narnya");
            given_dependency("depends: vilya");
            given_project_package("narnya", "1.0.0");
            given_project_package("vilya", "1.0.0");

            given_remote_package("narnya", "2.0.0");
            given_remote_package("vilya", "2.0.0");

            when_executing_command("narnya");
        }

        [Test]
        public void named_is_updated()
        {
            Environment.ProjectRepository.ShouldHavePackage("narnya", "2.0.0");
        }

        [Test]
        public void unrelated_should_not_be_updated()
        {
            Environment.ProjectRepository.ShouldNotHavePackage("vilya", "2.0.0");
        }
    }
}