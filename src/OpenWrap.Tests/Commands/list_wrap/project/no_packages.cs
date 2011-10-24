using NUnit.Framework;
using OpenWrap.Commands.Wrap;
using OpenWrap.Testing;

namespace Tests.Commands.list_wrap.project
{
    public class no_packages : command<ListWrapCommand>
    {
        public no_packages()
        {
            given_project_repository();
            when_executing_command();
        }
        [Test]
        public void no_packages_message_is_displayed()
        {
            Results.ShouldHaveOne<NoPackages>()
                .Check(_=>_.Repositories.ShouldHaveCountOf(1))
                .Check(_=>_.Search.ShouldBeNull());

        }
    }
}