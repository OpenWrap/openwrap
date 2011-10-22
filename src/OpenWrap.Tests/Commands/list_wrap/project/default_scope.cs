using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Commands.Wrap;
using OpenWrap.Testing;

namespace Tests.Commands.list_wrap.project
{
    public class default_scope : command<ListWrapCommand>
    {
        public default_scope()
        {
            given_project_package("sauron", "1.0.0");
            given_project_package("sauron", "1.0.1", "depends: one-ring");
            given_project_package("one-ring", "1.0.1");
            given_dependency("depends: sauron");
            when_executing_command();

        }
        [Test]
        public void selected_pacakge_is_displayed()
        {
            Results.ShouldHaveOne<DescriptorPackages>()
                .Check(_ => _.DescriptorName.ShouldBe("default scope"))
                .Check(_=>_.Packages.Single().Spec.ShouldBe("sauron"))
                .Check(_=>_.Packages.Single().PackageInfo.Version.ShouldBe("1.0.1".ToVersion()));
        }
    }
}