using NUnit.Framework;
using OpenWrap;
using Tests.Commands.update_wrap.project;

namespace Tests.Commands.add_wrap.locks
{
    public class lock_is_respected : contexts.add_wrap
    {
        public lock_is_respected()
        {
            given_project_package("sauron", "1.0.0");
            given_locked_package("sauron", "1.0.0");

            given_remote_package("one-ring", "1.0.0", "depends: sauron");
            given_remote_package("sauron", "2.0.0");

            given_dependency("depends: sauron");

            when_executing_command("one-ring");
        }

        [Test]
        public void package_stays_locked()
        {
            Environment.ProjectRepository.ShouldHavePackage("sauron", "1.0.0");
            Environment.ProjectRepository.ShouldNotHavePackage("sauron", "2.0.0");
        }
    }
}