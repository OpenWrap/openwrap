using System.Linq;
using NUnit.Framework;
using OpenWrap.Commands.Wrap;
using OpenWrap.Testing;

namespace Tests.Commands.list_wrap.project
{
    public class filtering_with_casing : command<ListWrapCommand>
    {
        public filtering_with_casing()
        {
            given_project_package("one-ring", "1.0");
            given_dependency("depends: one-ring");

            when_executing_command("*Rin*");
        }
        [Test]
        public void casing_is_ignored()
        {
            Results.OfType<DescriptorPackages>()
                .Single().Packages.Single().PackageInfo.Name.ShouldBe("one-ring");
        }
    }
}