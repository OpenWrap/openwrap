using NUnit.Framework;
using OpenWrap.Repositories;

namespace Tests.Commands.unlock_wrap.name
{
    public class ignore_dependencies : contexts.unlock_wrap
    {
        public ignore_dependencies()
        {
            given_project_repository(new InMemoryRepository { CanLock = true });

            given_project_package("sauron", "1.0.0.0");
            given_project_package("one-ring", "2.0.1.3", "depends: sauron");

            given_locked_package("one-ring", "2.0.1.3");
            given_locked_package("sauron", "1.0.0.0");
            
            given_dependency("depends: one-ring");

            when_executing_command("-name one-ring -ignoredependencies");
        }

        [Test]
        public void command_succeeds()
        {
            Results.ShouldHaveNoError();
        }
        [Test]
        public void package_is_unlocked()
        {
            project_repo.ShouldNotHaveLock("one-ring");
        }

        [Test]
        public void dependent_package_is_not_unlocked()
        {
            project_repo.ShouldHaveLock("sauron", "1.0.0.0");

        }
    }
}