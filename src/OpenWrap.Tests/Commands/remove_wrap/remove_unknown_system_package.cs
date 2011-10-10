using NUnit.Framework;

namespace Tests.Commands.remove_wrap
{
    public class remove_unknown_system_package : global::Tests.Commands.contexts.remove_wrap
    {
        public remove_unknown_system_package()
        {
            when_executing_command("saruman -system");
        }
        [Test]
        public void an_error_is_triggered()
        {
            Results.ShouldHaveError();
        }
    }
}