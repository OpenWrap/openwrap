using NUnit.Framework;
using Tests.Commands.update_wrap.project;

namespace Tests.Commands.remove_wrap
{
    public class removing_last_version : global::Tests.Commands.contexts.remove_wrap
    {
        public removing_last_version()
        {
            given_system_package("saruman", "1.0.0.0");
            given_system_package("saruman", "1.0.0.1");
            when_executing_command("saruman -system -last");
        }
        [Test]
        public void packge_is_removed()
        {
            Environment.SystemRepository.ShouldNotHavePackage("saruman", "1.0.0.1");
        }
        [Test]
        public void earlier_versions_are_preserved()
        {
            Environment.SystemRepository.ShouldHavePackage("saruman", "1.0.0.0");
        }
    }
}