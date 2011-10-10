using System;
using NUnit.Framework;
using OpenWrap.Commands.Wrap;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests.Commands.unlock_wrap
{
    public class all : contexts.unlock_wrap
    {
        public all()
        {
            given_project_repository(new InMemoryRepository { CanLock = true });
            
            given_project_package("one-ring", "2.0.1.3");
            given_locked_package("one-ring", "2.0.1.3");
            given_dependency("depends: one-ring");
            when_executing_command();

        }

        [Test]
        public void package_is_unlocked()
        {
            project_repo.LockedPackages[string.Empty].ShouldBeEmpty();
        }

        [Test]
        public void message_is_output()
        {
            Results.ShouldHaveOne<PackageUnlocked>()
                .Package.Name.ShouldBe("one-ring");
        }
    }
}