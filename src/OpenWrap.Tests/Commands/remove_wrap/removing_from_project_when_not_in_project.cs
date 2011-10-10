using NUnit.Framework;

namespace Tests.Commands.remove_wrap
{
    public class removing_from_project_when_not_in_project : global::Tests.Commands.contexts.remove_wrap
    {
        public removing_from_project_when_not_in_project()
        {
            given_project_repository(null);
            when_executing_command("saruman");
        }
        [Test]
        public void triggers_an_error()
        {
            Results.ShouldHaveError();
        }
    }
}
