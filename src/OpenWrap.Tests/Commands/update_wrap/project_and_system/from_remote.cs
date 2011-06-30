using NUnit.Framework;
using OpenWrap;
using OpenWrap.Commands.Wrap;
using OpenWrap.Tests.Commands;
using Tests.Commands.contexts;

namespace Tests.Commands.update_wrap.project_and_system
{
    public class from_remote: contexts.update_wrap
    {
        
        public from_remote()
        {
            given_dependency("depends: goldberry");


            given_project_package("goldberry", "2.0.0");
            given_system_package("goldberry", "2.0.0");
            given_remote_package("goldberry", "3.0.0".ToVersion());


            when_executing_command("-system -project");
        }
        [Test]
        public void project_repo_updated()
        {
            Environment.ProjectRepository.ShouldHavePackage("goldberry", "3.0.0");
        }
        [Test]
        public void system_repo_updated()
        {
            Environment.SystemRepository.ShouldHavePackage("goldberry", "3.0.0");
            
        }
    }
}