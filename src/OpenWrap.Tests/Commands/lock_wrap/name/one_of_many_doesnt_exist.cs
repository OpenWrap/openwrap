using NUnit.Framework;
using OpenWrap.Commands.Wrap;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests.Commands.lock_wrap.name
{
    public class one_of_many_doesnt_exist : contexts.lock_wrap
    {
        public one_of_many_doesnt_exist()
        {
            given_project_repository(new InMemoryRepository { CanLock = true });
            given_project_package("one-ring", "1.0.0");
            given_dependency("depends: one-ring");

            when_executing_command("one-ring, sauron");
        }

        [Test]
        public void error_is_reported()
        {
            Results.ShouldHaveOne<PackageNotFound>().PackageName.ShouldBe("sauron");
        }
    }
}