using NUnit.Framework;
using Tests.Commands;
using Tests.Commands.contexts;

namespace OpenWrap.Commands.remove_wrap
{
    public class removing_from_project_when_not_in_project : remove_wrap_command
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
