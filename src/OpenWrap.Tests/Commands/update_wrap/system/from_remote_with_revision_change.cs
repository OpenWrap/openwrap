using NUnit.Framework;
using OpenWrap;
using OpenWrap.Commands.contexts;
using OpenWrap.Commands.Wrap;
using OpenWrap.Tests.Commands;

namespace Tests.Commands.update_wrap.system
{
    public class from_remote_with_revision_change : command_context<UpdateWrapCommand>
    {
        public from_remote_with_revision_change()
        {
            given_project_package("goldberry", "2.0.0");
            given_system_package("goldberry", "2.1.0.0");
            given_remote_package("goldberry", "2.1.0.1".ToVersion());
            
            when_executing_command("-system");
        }
        [Test]
        public void project_repo_not_updated()
        {
            Environment.ProjectRepository.ShouldHavePackage("goldberry","2.0.0");
        }

        [Test]
        public void system_repo_updated()
        {
            Environment.SystemRepository.ShouldHavePackage("goldberry", "2.1.0.1");            
        }
    }
}