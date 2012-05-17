using NUnit.Framework;
using OpenWrap;
using OpenWrap.Tests.Commands;
using Tests.Commands.update_wrap.project;

namespace Tests.Commands.update_wrap.system
{
    public class from_remote_with_revision_change : contexts.update_wrap
    {
        public from_remote_with_revision_change()
        {
            given_project_package("goldberry", "2.0.0");
            given_system_package("goldberry", "2.1.0.0");
            given_remote_package("goldberry", "2.1.0.1");
            
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