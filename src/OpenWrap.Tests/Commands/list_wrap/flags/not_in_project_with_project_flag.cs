using NUnit.Framework;
using OpenWrap.Commands.Wrap;
using OpenWrap.Testing;

namespace Tests.Commands.list_wrap.flags
{
    public class not_in_project_with_project_flag : command<ListWrapCommand>
    {
        public not_in_project_with_project_flag()
        {
            when_executing_command("-project");

        }
        [Test]
        public void error_is_returned()
        {
            Results.ShouldHaveOne<NotInProject>();
        }
    }
}