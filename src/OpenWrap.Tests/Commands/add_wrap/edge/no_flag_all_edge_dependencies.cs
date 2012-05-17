using NUnit.Framework;
using OpenWrap;

namespace Tests.Commands.add_wrap.edge
{
    public class no_flag_all_edge_dependencies : contexts.add_wrap
    {
        public no_flag_all_edge_dependencies()
        {
            given_project_repository();
            given_remote_package("one-ring", "2.0.0-beta");
            given_remote_package("one-ring", "1.0.0");
            given_remote_package("sauron", "2.0.0-beta", "depends: one-ring edge");
            given_remote_package("sauron", "1.0.0", "depends: one-ring");
            when_executing_command("sauron");
        }

        [Test]
        public void release_is_used()
        {
            package_is_in_repository(Environment.ProjectRepository, "sauron", "1.0.0".ToSemVer());
        }

        [Test]
        public void release_dependency_is_used()
        {
            package_is_in_repository(Environment.ProjectRepository, "one-ring", "1.0.0".ToSemVer());

        }
    }
}