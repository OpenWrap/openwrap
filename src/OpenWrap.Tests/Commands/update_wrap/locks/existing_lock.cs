using NUnit.Framework;
using OpenWrap;
using Tests.Commands.update_wrap.project;

namespace Tests.Commands.update_wrap.locks
{
    public class existing_lock : contexts.update_wrap
    {
        public existing_lock()
        {
            
            given_dependency("depends: vilya");
            given_project_package("narnya", "1.0.0.0");
            given_project_package("vilya", "1.0.0.0", "depends: narnya");
            given_locked_package("narnya", "1.0.0.0");

            given_remote_package("narnya", "1.0.0.1".ToVersion());
            given_remote_package("vilya", "1.0.0.1".ToVersion());

            when_executing_command();
        }

        [Test]
        public void unlocked_package_is_updated()
        {
            Environment.ProjectRepository.ShouldHavePackage("vilya", "1.0.0.1");
        }

        [Test]
        public void locked_package_is_not_updated()
        {
            Environment.ProjectRepository.ShouldNotHavePackage("narnya", "1.0.0.1");
        }
    }
}