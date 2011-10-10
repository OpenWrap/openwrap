using NUnit.Framework;

namespace Tests.Commands.remove_wrap
{
    public class removing_unknown_package_in_project : global::Tests.Commands.contexts.remove_wrap
    {
        public removing_unknown_package_in_project()
        {
            when_executing_command("saruman");
        }
        [Test]
        public void an_error_is_triggered()
        {
            Results.ShouldHaveError();
        }
    }
}