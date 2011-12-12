using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Commands.Wrap;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests.Commands.unlock_wrap.name
{
    public class with_dependents : contexts.unlock_wrap
    {
        public with_dependents()
        {
            given_project_repository(new InMemoryRepository { CanLock = true });

            given_project_package("frodo", "1.0.0.0");
            given_project_package("sauron", "1.0.0.0");
            given_project_package("one-ring", "2.0.1.3", "depends: sauron");
            // make sure the locks are respected when resolving
            given_project_package("one-ring", "2.0.1.4", "depends: sauron");

            given_locked_package("one-ring", "2.0.1.3");
            given_locked_package("sauron", "1.0.0.0");
            given_locked_package("frodo", "1.0.0.0");

            given_dependency("depends: one-ring");
            given_dependency("depends: frodo");

            when_executing_command("-name one-ring");
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
        public void dependent_package_is_unlocked()
        {
            project_repo.ShouldNotHaveLock("sauron");

        }

        [Test]
        public void unaffected_package_is_not_unlocked()
        {
            project_repo.ShouldHaveLock("frodo", "1.0.0.0");
        }

        [Test]
        public void unlock_message_is_output()
        {
            Results.OfType<PackageUnlocked>().ShouldHaveCountOf(2)
                .ShouldHaveOne(_ => _.Package.Name == "sauron" && _.Package.Version == "1.0.0.0".ToSemVer())
                .ShouldHaveOne(_ => _.Package.Name == "one-ring" && _.Package.Version == "2.0.1.3".ToSemVer());
        }
    }
}