using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Commands.Wrap;
using OpenWrap.Testing;

namespace Tests.Commands.list_wrap.project
{
    public class multiple_scopes : command<ListWrapCommand>
    {
        public multiple_scopes()
        {
            given_project_package("sauron", "1.0.0");
            given_project_package("sauron", "1.0.1");
            given_dependency("depends: sauron = 1.0.1");
            given_dependency("tests", "depends: sauron = 1.0.0");
            when_executing_command();

        }
        [Test]
        public void selected_pacakge_for_default_is_displayed()
        {
            Results.OfType<DescriptorPackages>()
                .ElementAt(0)
                .Check(_ => _.DescriptorName.ShouldBe("default scope"))
                .Check(_ => _.Packages.First().Spec.ShouldBe("sauron = 1.0.1"))
                .Check(_ => _.Packages.First().PackageInfo.Version.ShouldBe("1.0.1".ToSemVer()));
        }
        [Test]
        public void selected_pacakge_for_different_scope_is_displayed()
        {
            Results.OfType<DescriptorPackages>()
                .ElementAt(1)
                .Check(_ => _.DescriptorName.ShouldBe("tests scope"))
                .Check(_ => _.Packages.First().Spec.ShouldBe("sauron = 1.0.0"))
                .Check(_ => _.Packages.First().PackageInfo.Version.ShouldBe("1.0.0".ToSemVer()));
        }
    }
}