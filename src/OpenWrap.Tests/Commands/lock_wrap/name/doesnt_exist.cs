using NUnit.Framework;
using OpenWrap.Commands.Wrap;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests.Commands.lock_wrap.name
{
    public class doesnt_exist : contexts.lock_wrap
    {
        public doesnt_exist()
        {

            given_project_repository(new InMemoryRepository { CanLock = true });

            when_executing_command("one-ring");
        }

        [Test]
        public void error_is_reported()
        {
            Results.ShouldHaveOne<PackageNotFound>().PackageName.ShouldBe("one-ring");
        }
    }
}