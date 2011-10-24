using NUnit.Framework;
using OpenWrap.Commands.Wrap;
using OpenWrap.Testing;

namespace Tests.Commands.list_wrap.project
{
    public class no_packages_by_name : command<ListWrapCommand>
    {
        public no_packages_by_name()
        {
            given_project_package("sauron", "1.0");
            given_dependency("depends: sauron");
            when_executing_command("one-ring");
        }
        [Test]
        public void no_packages_message_is_displayed()
        {
            Results.ShouldHaveOne<NoPackages>()
                .Check(_ => _.Repositories.ShouldHaveCountOf(1))
                .Check(_ => _.Search.ShouldBe("one-ring"));

        }
    }
}