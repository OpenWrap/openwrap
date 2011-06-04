using NUnit.Framework;
using OpenWrap;
using OpenWrap.Commands.contexts;
using OpenWrap.Commands.Wrap;
using Tests.Commands.contexts;

namespace Tests.Commands.update_wrap.system
{
    public class from_remote_by_name: contexts.update_wrap
    {
        public from_remote_by_name()
        {
            given_system_package("goldberry", "2.0");
            given_system_package("one-ring", "1.0");
            given_remote_package("one-ring", "1.1".ToVersion());
            given_remote_package("goldberry", "2.1".ToVersion());

            when_executing_command("one-ring -sys");
        }
        [Test]
        public void project_is_updated()
        {
            Environment.SystemRepository.ShouldHavePackage("one-ring", "1.1");
        }
        [Test]
        public void not_selected_projects_are_not_updated()
        {
            Environment.SystemRepository.ShouldNotHavePackage("goldberry", "2.1");
        }
    }
}