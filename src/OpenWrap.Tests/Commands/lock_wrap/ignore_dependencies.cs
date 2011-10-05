using NUnit.Framework;
using OpenWrap.Repositories;

namespace Tests.Commands.lock_wrap
{
    public class ignore_dependencies : contexts.lock_wrap
    {
        public ignore_dependencies()
        {
            given_project_repository(new InMemoryRepository { CanLock = true });
            given_project_package("sauron", "1.0.0.0");
            given_project_package("one-ring", "1.0.0", "depends: sauron = 1.0.0");
            given_project_package("frodo", "1.0.0");
            given_dependency("depends: one-ring");
            given_dependency("depends: frodo");

            when_executing_command("-ignoredependencies");
        }

        [Test]
        public void packages_from_descriptor_are_locked()
        {
            project_repo.ShouldHaveLock("one-ring", "1.0.0");
            project_repo.ShouldHaveLock("frodo", "1.0.0");
        }
        [Test]
        public void dependent_package_is_not_locked()
        {
            project_repo.ShouldNotHaveLock("sauron");
        }
    }
}