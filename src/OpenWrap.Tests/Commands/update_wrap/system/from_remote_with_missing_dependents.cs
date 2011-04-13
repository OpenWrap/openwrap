using NUnit.Framework;
using OpenWrap;
using OpenWrap.Commands.contexts;
using OpenWrap.Commands.Wrap;

namespace Tests.Commands.update_wrap.system
{
    public class from_remote_with_missing_dependents : command_context<UpdateWrapCommand>
    {
        public from_remote_with_missing_dependents()
        {
            given_system_package("nurn", "2.1.0.0");

            given_remote_package("nurn", "2.1.1.0".ToVersion(), "depends: mordor");

            when_executing_command("nurn", "-system");
        }

        [Test]
        public void system_repo_not_updated()
        {
            Environment.SystemRepository.ShouldHavePackage("nurn", "2.1.0.0");
        }

        [Test]
        public void warning_is_generated()
        {
            Results.ShouldHaveWarning();
        }
    }
}