using NUnit.Framework;
using Tests.Commands;
using Tests.Commands.contexts;

namespace OpenWrap.Commands.remove_wrap
{
    public class removing_unknown_package_in_project : remove_wrap_command
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