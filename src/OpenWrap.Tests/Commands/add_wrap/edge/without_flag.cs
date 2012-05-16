using NUnit.Framework;
using OpenWrap;

namespace Tests.Commands.add_wrap.edge
{
    public class without_flag : contexts.add_wrap
    {
        public without_flag()
        {
            given_project_repository();
            given_remote_package("sauron", "1.0.0");
            given_remote_package("sauron", "2.0.0-beta");
            when_executing_command("sauron");
        }

        [Test]
        public void non_beta_should_be_used()
        {
            package_is_in_repository(Environment.ProjectRepository, "sauron", "1.0.0".ToSemVer());
        }
    }
}