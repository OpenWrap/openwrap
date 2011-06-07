using NUnit.Framework;
using OpenWrap.Commands.Wrap;
using Tests.Commands.contexts;

namespace Tests.Commands.update_wrap.project
{
    public class package_only_in_project: contexts.update_wrap
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
