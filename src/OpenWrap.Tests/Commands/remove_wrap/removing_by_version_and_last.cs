using NUnit.Framework;

namespace Tests.Commands.remove_wrap
{
    public class removing_by_version_and_last : global::Tests.Commands.contexts.remove_wrap
    {
        public removing_by_version_and_last()
        {
            given_system_package("saruman", "1.0.0.0");
            when_executing_command("saruman -version 1.0.0.0 -last");
        }
        [Test]
        public void error_is_displayed()
        {
            Results.ShouldHaveError();
        }
    }
}