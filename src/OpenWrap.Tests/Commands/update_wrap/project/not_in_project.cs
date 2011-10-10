using NUnit.Framework;
using OpenWrap;
using OpenWrap.Commands.Wrap;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace Tests.Commands.update_wrap.project
{
    public class not_in_project: contexts.update_wrap
    {
        public not_in_project()
        {
            given_system_package("goldberry", "2.0.0");
            given_remote_package("goldberry", "2.1.0".ToVersion());

            when_executing_command();
        }
        [Test]
        public void error_message_is_generated()
        {
            Results.ShouldHaveOne<NotInProject>();
        }
        [Test]
        public void package_in_system_repository_is_not_updated()
        {
            Environment.SystemRepository.ShouldHavePackage("goldberry", "2.0.0");
        }
    }
}