using System.Linq;
using NUnit.Framework;
using OpenWrap.Commands.Wrap;
using OpenWrap.Testing;

namespace Tests.Commands.list_wrap.project
{
    public class locked_package : command<ListWrapCommand>
    {
        public locked_package()
        {
            given_project_package("sauron", "1.0.0");
            given_project_package("sauron", "1.0.1");
            given_dependency("depends: sauron");
            given_locked_package("sauron", "1.0.0");
            when_executing_command();

        }
        [Test]
        public void selected_pacakge_is_displayed()
        {
            Results.ShouldHaveOne<DescriptorPackages>()
                .Check(_ => _.DescriptorName.ShouldBe("default scope"))
                .Check(_ => _.Packages.First().Locked.ShouldBeTrue());
        }
    }
}