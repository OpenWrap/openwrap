using NUnit.Framework;
using OpenWrap.Commands.contexts;
using OpenWrap.Commands.Wrap;

namespace Tests.Commands.update_wrap.system
{
    public class by_name_missing_package : command_context<UpdateWrapCommand>
    {
        public by_name_missing_package()
        {
            given_system_package("nurn", "2.1.0.0");
            when_executing_command("nurn -system");
        }

        [Test]
        public void an_error_is_generated()
        {
            Results.ShouldHaveError();
        }
    }
}