using NUnit.Framework;
using OpenWrap.Commands.Wrap;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests.Commands.lock_wrap
{
    public class locking_not_enabled : contexts.lock_wrap
    {
        public locking_not_enabled()
        {
            given_project_repository(new InMemoryRepository { CanLock = false });
            when_executing_command();
        }

        [Test]
        public void error_is_displayed()
        {
            Results.ShouldHaveOne<LockingNotSupported>()
                .Repository.ShouldBe(Environment.ProjectRepository);
        }
    }
}