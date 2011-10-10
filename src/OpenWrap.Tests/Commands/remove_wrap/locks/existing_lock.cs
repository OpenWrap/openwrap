using NUnit.Framework;
using OpenWrap.Commands.Wrap;
using OpenWrap.Testing;
using Tests.Commands.update_wrap.project;

namespace Tests.Commands.remove_wrap.locks
{
    public class existing_lock : contexts.remove_wrap
    {
        public existing_lock()
        {
            given_dependency("depends: vilya");
            given_project_package("vilya", "1.0.0.0");
            given_locked_package("vilya", "1.0.0.0");

            when_executing_command("vilya");
        }

        [Test]
        public void package_not_removed()
        {
            PostCommandDescriptor.Dependencies.ShouldHaveOne(_ => _.Name == "vilya");
        }

        [Test]
        public void error_message_displayed()
        {
            Results.ShouldHaveOne<PackageLockedNotRemoved>()
                .PackageName.ShouldBe("vilya");
        }
    }
}