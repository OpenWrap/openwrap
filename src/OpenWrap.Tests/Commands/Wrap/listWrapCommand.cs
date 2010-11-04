using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenWrap.Commands.Wrap;
using OpenWrap.Testing;
using OpenWrap.Tests.Commands.context;

namespace listWrap_specs
{
    public class filtering_project_package_list_by_name : command_context<ListWrapCommand>
    {
        public filtering_project_package_list_by_name()
        {
            given_project_package("one-ring", "1.0");
            given_project_package("sauron", "2.0");
            given_dependency("depends: one-ring");
            given_dependency("depends: sauron");

            when_executing_command("one*");
        }
        [Test]
        public void matching_package_is_returned()
        {
            Results.OfType<PackageDescriptionOutput>()
                    .ShouldHaveCountOf(1)
                    .First().PackageName.ShouldBe("one-ring");
        }
    }
    public class filtering_project_with_different_casing : command_context<ListWrapCommand>
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
            Results.OfType<PackageDescriptionOutput>()
                    .ShouldHaveCountOf(1)
                    .First().PackageName.ShouldBe("one-ring");
        }
    }
}
