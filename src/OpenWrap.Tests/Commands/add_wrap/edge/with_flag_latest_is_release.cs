using NUnit.Framework;
using OpenWrap;

namespace Tests.Commands.add_wrap.edge
{
    public class with_flag_latest_is_release : contexts.add_wrap
    {
        public with_flag_latest_is_release()
        {
            given_project_repository();
            given_remote_package("sauron", "2.0.0-beta");
            given_remote_package("sauron", "2.0.0");
            when_executing_command("sauron -edge");
        }

        [Test]
        public void release_is_used()
        {
            package_is_in_repository(Environment.ProjectRepository, "sauron", "2.0.0".ToSemVer());
        }
    }
}