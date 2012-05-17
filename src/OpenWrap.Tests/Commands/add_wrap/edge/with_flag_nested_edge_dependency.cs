using NUnit.Framework;
using OpenWrap;

namespace Tests.Commands.add_wrap.edge
{
    public class with_flag_nested_edge_dependency : contexts.add_wrap
    {
        public with_flag_nested_edge_dependency()
        {
            given_project_repository();
            given_remote_package("one-ring", "2.0.0-beta");
            given_remote_package("one-ring", "1.0.0");
            given_remote_package("sauron", "2.0.0", "depends: one-ring edge");
            given_remote_package("sauron", "1.0.0", "depends: one-ring");
            when_executing_command("sauron -edge");
        }

        [Test]
        public void release_is_used()
        {
            package_is_in_repository(Environment.ProjectRepository, "sauron", "2.0.0".ToSemVer());
        }

        [Test]
        public void release_dependency_is_used()
        {
            package_is_in_repository(Environment.ProjectRepository, "one-ring", "2.0.0-beta".ToSemVer());

        }
    }
}