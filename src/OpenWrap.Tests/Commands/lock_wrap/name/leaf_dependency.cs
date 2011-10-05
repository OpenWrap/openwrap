using NUnit.Framework;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests.Commands.lock_wrap.name
{
    public class leaf_dependency : contexts.lock_wrap
    {
        public leaf_dependency()
        {
            given_project_repository(new InMemoryRepository { CanLock = true });
            given_project_package("sauron", "1.0.0.0");
            given_project_package("one-ring", "1.0.0", "depends: sauron = 1.0.0");
            given_project_package("frodo", "1.0.0");
            given_dependency("depends: one-ring");
            given_dependency("depends: frodo");

            when_executing_command("sauron");
        }

        [Test]
        public void package_is_locked()
        {
            project_repo.ShouldHaveLock("sauron", "1.0.0.0");
        }

        [Test]
        public void no_other_package_is_locked()
        {
            project_repo.LockedPackages[string.Empty].ShouldHaveCountOf(1);
        }
    }
}