using NUnit.Framework;
using OpenWrap;
using Tests.Commands.update_wrap.project;

namespace Tests.Commands.update_wrap.system
{
    public class from_remote_when_already_at_latest: contexts.update_wrap
    {
        public from_remote_when_already_at_latest()
        {
            given_system_package("goldberry", "2.2.0");
            given_remote_package("goldberry", "2.2.0".ToVersion());
            when_executing_command("-system");
        }

        [Test]
        public void system_repo_not_updated()
        {
            Environment.SystemRepository.ShouldHavePackage("goldberry", "2.2.0");
        }

        [Test]
        public void no_errors_are_generated()
        {
            Results.ShouldHaveNoError();
        }

        [Test]
        public void no_warnings_are_generated()
        {
            Results.ShouldHaveNoWarning();
        }
    }
}