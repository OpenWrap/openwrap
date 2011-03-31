using NUnit.Framework;
using OpenWrap.Commands.contexts;
using OpenWrap.Commands.Wrap;

namespace Tests.Commands.update_wrap.project
{
    public class package_only_in_project : command_context<UpdateWrapCommand>
    {
        public package_only_in_project()
        {
            given_dependency("depends: goldberry");
            given_project_package("goldberry", "1.0");

            when_executing_command();
        }
        [Test]
        public void error_is_reported()
        {
            Results.ShouldHaveError();
        }
    }
}
