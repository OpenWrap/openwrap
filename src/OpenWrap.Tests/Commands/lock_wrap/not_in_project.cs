using NUnit.Framework;
using OpenWrap.Commands.Wrap;
using OpenWrap.Testing;

namespace Tests.Commands.lock_wrap
{
    public class not_in_project : contexts.lock_wrap
    {
        public not_in_project()
        {
            when_executing_command();
        }

        [Test]
        public void error_is_displayed()
        {
            Results.ShouldHaveOne<NotInProject>();
        }
    }
}