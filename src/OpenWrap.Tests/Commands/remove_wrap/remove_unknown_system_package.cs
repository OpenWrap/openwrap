using NUnit.Framework;
using Tests.Commands;
using Tests.Commands.contexts;

namespace OpenWrap.Commands.remove_wrap
{
    public class remove_unknown_system_package : remove_wrap_command
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