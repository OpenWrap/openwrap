using NUnit.Framework;
using OpenWrap.Commands.Wrap;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests.Commands.unlock_wrap
{
    public class repository_deosnt_supoort_locking : contexts.unlock_wrap
    {
        public repository_deosnt_supoort_locking()
        {
            given_project_repository(new InMemoryRepository());
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