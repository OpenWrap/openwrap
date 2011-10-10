using NUnit.Framework;
using Tests.Commands.update_wrap.project;

namespace Tests.Commands.remove_wrap
{
    public class removing_by_version_in_system : global::Tests.Commands.contexts.remove_wrap
    {
        public removing_by_version_in_system()
        {
            given_system_package("saruman", "1.0.0.0");
            given_system_package("saruman", "1.0.0.1");
            when_executing_command("saruman -system -version 1.0.0.1");
        }

        [Test]
        public void command_succeeds()
        {
            Results.ShouldHaveNoError();
        }
        [Test]
        public void version_is_removed()
        {
            Environment.SystemRepository
                    .ShouldNotHavePackage("saruman", "1.0.0.1");
        }
        [Test]
        public void other_versions_not_removed()
        {
            Environment.SystemRepository
                    .ShouldHavePackage("saruman", "1.0.0.0");
        }
    }
}