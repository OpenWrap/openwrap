using System.Linq;
using NUnit.Framework;
using OpenWrap.Commands.contexts;
using OpenWrap.Commands.Wrap;
using OpenWrap.PackageManagement;
using OpenWrap.Testing;
using Tests.Commands.contexts;
using Enumerable = System.Linq.Enumerable;

namespace listWrap_specs
{
    public class filtering_project_with_different_casing : command<ListWrapCommand>
    {
        public filtering_project_with_different_casing()
        {
            given_project_package("one-ring", "1.0");
            given_dependency("depends: one-ring");

            when_executing_command("*Rin*");
        }
        [Test]
        public void casing_is_ignored()
        {
            Results.OfType<PackageFoundCommandOutput>()
                    .ShouldHaveCountOf(1).First().Name.ShouldBe("one-ring");
        }
    }
}