using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Commands.Wrap;
using OpenWrap.Testing;

namespace Tests.Commands.list_wrap.remotes
{
    public class no_packages_by_name : command<ListWrapCommand>
    {
        public no_packages_by_name()
        {
            given_remote_repository("the-shire");
            given_remote_package("the-shire", "frodo", "1.0".ToVersion());
            when_executing_command("sauron -remote the-shire");
        }
        [Test]
        public void no_packages_message_is_displayed()
        {
            Results.ShouldHaveOne<NoPackages>()
                .Check(_ => _.Repositories.ShouldHaveCountOf(1).First().Name.ShouldBe("the-shire"))
                .Check(_ => _.Search.ShouldBe("sauron"));
            ;

        }
    }
}