using NUnit.Framework;
using OpenWrap.Commands.Wrap;
using Tests.Commands.contexts;

namespace Tests.Commands.update_wrap.system
{
    public class by_name_missing_package: contexts.update_wrap
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